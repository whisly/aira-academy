/* ============================================================
   Aira Academy — seed data
   Mirror of the in-memory seed in Data/WorkshopRepository.cs
   ============================================================ */

USE AiraAcademy;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRANSACTION;

/* ---------------------------------------------------------- workshops */

MERGE dbo.Workshops AS target
USING (VALUES
    ('one-person-company', 'OPC-01', N'One Person Company AI Workshop',
     N'Build your own website, a working sales backend and a WhatsApp auto-reply in two days.',
     N'Most AI courses leave you with notes. This one leaves you with a running business system. You bring your product and your customers; we build the site, the order flow and the follow-up automation together, on your own laptop, before you leave the room.',
     N'Founders, freelancers and solo operators', N'Beginner friendly', 3480, N'for two people', 2, 1, 'orange'),

    ('people-ops', 'HR-01', N'People Operations AI Workshop',
     N'Cut the administrative half of HR without cutting the human half.',
     N'Screening, scheduling, policy questions and onboarding paperwork all follow patterns. This workshop shows you where AI genuinely helps, where it quietly creates risk, and how to keep a person in the loop on anything that affects someone''s job.',
     N'HR managers, recruiters and office administrators', N'Beginner friendly', 2680, N'per person', 2, 1, 'teal'),

    ('finance-operations', 'FIN-01', N'Finance Operations AI Workshop',
     N'Reconcile, categorise and draft — with a reviewer still in the chair.',
     N'Finance teams do not need faster guesses; they need less manual sorting. This workshop is built around documents you already have: invoices, statements, receipts and month-end packs. Every workflow ends at a human approval step, on purpose.',
     N'Accountants, bookkeepers and finance leads', N'Intermediate', 2680, N'per person', 2, 1, 'plum'),

    ('corporate-team', 'CORP-01', N'Corporate Team AI Programme',
     N'One department, one real workflow, measured in weeks not quarters.',
     N'A private programme for management teams. We pick one workflow that already hurts, map it with your people, build the automation with them, and leave behind a measured before-and-after rather than a slide deck.',
     N'Department heads and management teams', N'Leadership', 38000, N'per team, scoped', 5, 0, 'ink')
) AS source (Slug, Code, Title, Summary, Description, Audience, Level, PriceMyr, PriceNote, DurationDays, IsFeatured, Accent)
ON target.Slug = source.Slug
WHEN MATCHED THEN
    UPDATE SET Code = source.Code, Title = source.Title, Summary = source.Summary,
               Description = source.Description, Audience = source.Audience, Level = source.Level,
               PriceMyr = source.PriceMyr, PriceNote = source.PriceNote,
               DurationDays = source.DurationDays, IsFeatured = source.IsFeatured, Accent = source.Accent
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Slug, Code, Title, Summary, Description, Audience, Level, PriceMyr, PriceNote, DurationDays, IsFeatured, Accent)
    VALUES (source.Slug, source.Code, source.Title, source.Summary, source.Description, source.Audience,
            source.Level, source.PriceMyr, source.PriceNote, source.DurationDays, source.IsFeatured, source.Accent);

/* ---------------------------------------------------------- sessions */

;WITH s AS
(
    SELECT w.Id AS WorkshopId, v.StartsOn, v.EndsOn, v.City, v.SeatsLeft
    FROM (VALUES
        ('one-person-company', '2026-12-05', '2026-12-06', N'Johor',           6),
        ('one-person-company', '2026-12-12', '2026-12-13', N'Kuala Lumpur',    9),
        ('one-person-company', '2027-01-09', '2027-01-10', N'Penang',         12),
        ('people-ops',         '2026-12-08', '2026-12-09', N'Kuala Lumpur',    4),
        ('people-ops',         '2027-01-14', '2027-01-15', N'Johor',          14),
        ('finance-operations', '2026-12-15', '2026-12-16', N'Penang',          7),
        ('finance-operations', '2027-02-04', '2027-02-05', N'Kuala Lumpur',   16),
        ('corporate-team',     '2027-01-19', '2027-02-13', N'On site',         3)
    ) AS v (Slug, StartsOn, EndsOn, City, SeatsLeft)
    JOIN dbo.Workshops AS w ON w.Slug = v.Slug
)
MERGE dbo.WorkshopSessions AS target
USING s AS source
   ON target.WorkshopId = source.WorkshopId
  AND target.StartsOn  = CONVERT(DATE, source.StartsOn)
  AND target.City      = source.City
WHEN MATCHED THEN
    UPDATE SET EndsOn = CONVERT(DATE, source.EndsOn), SeatsLeft = source.SeatsLeft
WHEN NOT MATCHED BY TARGET THEN
    INSERT (WorkshopId, StartsOn, EndsOn, City, SeatsLeft)
    VALUES (source.WorkshopId, CONVERT(DATE, source.StartsOn), CONVERT(DATE, source.EndsOn),
            source.City, source.SeatsLeft);

/* ---------------------------------------------------------- testimonials */

DELETE FROM dbo.Testimonials;
INSERT INTO dbo.Testimonials (Quote, PersonName, Role, Initials, SortOrder)
VALUES
    (N'We stopped answering the same twelve questions on WhatsApp. The auto-reply handles them and I only see the ones that matter.',
     N'Farah Idris', N'Founder, Peranakan Bakehouse', 'FI', 10),
    (N'The part I valued most was the list of things we should not automate. Nobody else told us that.',
     N'Daniel Wong', N'People Lead, Kestrel Logistics', 'DW', 20),
    (N'Month-end used to take four days of sorting. It now takes one afternoon plus a review.',
     N'Priya Raman', N'Finance Manager, Trilight Services', 'PR', 30);

COMMIT TRANSACTION;
GO

SELECT  (SELECT COUNT(*) FROM dbo.Workshops)        AS Workshops,
        (SELECT COUNT(*) FROM dbo.WorkshopSessions) AS Sessions,
        (SELECT COUNT(*) FROM dbo.Testimonials)     AS Testimonials;
GO
