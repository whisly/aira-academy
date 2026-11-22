/* ============================================================
   Aira Academy — schema (SQL Server 2019+)
   Run order: 01-schema.sql -> 02-seed.sql -> 03-procedures.sql
   ============================================================ */

IF DB_ID('AiraAcademy') IS NULL
BEGIN
    CREATE DATABASE AiraAcademy;
END;
GO

USE AiraAcademy;
GO

/* ---------------------------------------------------------- workshops */

IF OBJECT_ID('dbo.Workshops', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Workshops
    (
        Id            INT             IDENTITY(1,1) NOT NULL,
        Slug          VARCHAR(80)     NOT NULL,
        Code          VARCHAR(16)     NOT NULL,
        Title         NVARCHAR(140)   NOT NULL,
        Summary       NVARCHAR(300)   NOT NULL,
        Description   NVARCHAR(2000)  NOT NULL,
        Audience      NVARCHAR(160)   NOT NULL,
        Level         NVARCHAR(40)    NOT NULL,
        PriceMyr      INT             NOT NULL,
        PriceNote     NVARCHAR(60)    NOT NULL CONSTRAINT DF_Workshops_PriceNote DEFAULT (''),
        DurationDays  TINYINT         NOT NULL,
        IsFeatured    BIT             NOT NULL CONSTRAINT DF_Workshops_IsFeatured DEFAULT (0),
        Accent        VARCHAR(20)     NOT NULL CONSTRAINT DF_Workshops_Accent DEFAULT ('orange'),
        CreatedAt     DATETIME2(0)    NOT NULL CONSTRAINT DF_Workshops_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Workshops PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Workshops_Slug UNIQUE (Slug),
        CONSTRAINT CK_Workshops_Price CHECK (PriceMyr >= 0),
        CONSTRAINT CK_Workshops_Duration CHECK (DurationDays BETWEEN 1 AND 30)
    );
END;
GO

/* ---------------------------------------------------------- sessions */

IF OBJECT_ID('dbo.WorkshopSessions', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.WorkshopSessions
    (
        Id          INT           IDENTITY(1,1) NOT NULL,
        WorkshopId  INT           NOT NULL,
        StartsOn    DATE          NOT NULL,
        EndsOn      DATE          NOT NULL,
        City        NVARCHAR(60)  NOT NULL,
        SeatsLeft   SMALLINT      NOT NULL,

        CONSTRAINT PK_WorkshopSessions PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_WorkshopSessions_Workshop FOREIGN KEY (WorkshopId)
            REFERENCES dbo.Workshops (Id) ON DELETE CASCADE,
        CONSTRAINT CK_WorkshopSessions_Range CHECK (EndsOn >= StartsOn),
        CONSTRAINT CK_WorkshopSessions_Seats CHECK (SeatsLeft >= 0)
    );

    -- Covers "next session per workshop" and the date-sorted listings.
    CREATE INDEX IX_WorkshopSessions_Workshop_Start
        ON dbo.WorkshopSessions (WorkshopId, StartsOn)
        INCLUDE (EndsOn, City, SeatsLeft);
END;
GO

/* ---------------------------------------------------------- testimonials */

IF OBJECT_ID('dbo.Testimonials', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Testimonials
    (
        Id         INT            IDENTITY(1,1) NOT NULL,
        Quote      NVARCHAR(600)  NOT NULL,
        PersonName NVARCHAR(80)   NOT NULL,
        Role       NVARCHAR(120)  NOT NULL,
        Initials   VARCHAR(4)     NOT NULL,
        SortOrder  SMALLINT       NOT NULL CONSTRAINT DF_Testimonials_Sort DEFAULT (0),

        CONSTRAINT PK_Testimonials PRIMARY KEY CLUSTERED (Id)
    );
END;
GO

/* ---------------------------------------------------------- enquiries */

IF OBJECT_ID('dbo.Enquiries', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Enquiries
    (
        Id           UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_Enquiries_Id DEFAULT (NEWSEQUENTIALID()),
        PersonName   NVARCHAR(80)     NOT NULL,
        Email        NVARCHAR(200)    NOT NULL,
        Phone        NVARCHAR(30)     NULL,
        WorkshopId   INT              NULL,
        Message      NVARCHAR(1200)   NOT NULL,
        Source       VARCHAR(40)      NOT NULL CONSTRAINT DF_Enquiries_Source DEFAULT ('web'),
        CreatedAt    DATETIME2(0)     NOT NULL CONSTRAINT DF_Enquiries_CreatedAt DEFAULT (SYSUTCDATETIME()),

        CONSTRAINT PK_Enquiries PRIMARY KEY NONCLUSTERED (Id),
        CONSTRAINT FK_Enquiries_Workshop FOREIGN KEY (WorkshopId)
            REFERENCES dbo.Workshops (Id),
        CONSTRAINT CK_Enquiries_Email CHECK (Email LIKE '%_@_%._%'),
        CONSTRAINT CK_Enquiries_Message CHECK (LEN(LTRIM(RTRIM(Message))) >= 10)
    );

    -- Enquiries are read newest-first, so the clustered key follows the read order.
    CREATE UNIQUE CLUSTERED INDEX CX_Enquiries_CreatedAt_Id
        ON dbo.Enquiries (CreatedAt DESC, Id);

    CREATE INDEX IX_Enquiries_Workshop
        ON dbo.Enquiries (WorkshopId)
        WHERE WorkshopId IS NOT NULL;
END;
GO

/* ---------------------------------------------------------- reporting view */

CREATE OR ALTER VIEW dbo.vw_WorkshopOverview
AS
WITH next_session AS
(
    SELECT  s.WorkshopId,
            s.StartsOn,
            s.EndsOn,
            s.City,
            s.SeatsLeft,
            ROW_NUMBER() OVER (PARTITION BY s.WorkshopId ORDER BY s.StartsOn) AS rn
    FROM    dbo.WorkshopSessions AS s
    WHERE   s.StartsOn >= CAST(SYSUTCDATETIME() AS DATE)
)
SELECT  w.Id,
        w.Slug,
        w.Code,
        w.Title,
        w.PriceMyr,
        w.DurationDays,
        w.IsFeatured,
        ns.StartsOn   AS NextStartsOn,
        ns.EndsOn     AS NextEndsOn,
        ns.City       AS NextCity,
        ns.SeatsLeft  AS NextSeatsLeft,
        (SELECT COUNT(*) FROM dbo.Enquiries AS e WHERE e.WorkshopId = w.Id) AS EnquiryCount
FROM    dbo.Workshops AS w
LEFT JOIN next_session AS ns
       ON ns.WorkshopId = w.Id
      AND ns.rn = 1;
GO
