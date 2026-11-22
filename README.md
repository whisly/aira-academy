# Aira Academy

A small, complete **ASP.NET Core MVC** site for a fictional AI-training company in Malaysia.
Built as a portfolio piece: it reads like an ordinary brochure site, but every layer underneath
is deliberate and readable — service boundaries, server-side validation, a repository seam, and
a hand-written T-SQL layer with stored procedures.

![stack](https://img.shields.io/badge/.NET-10-512BD4) ![stack](https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4) ![db](https://img.shields.io/badge/SQL_Server-T--SQL-CC2927) ![deps](https://img.shields.io/badge/NuGet_dependencies-none-success)

---

## What is actually implemented

| Area | Detail |
|---|---|
| **Routing** | Attribute routes — `/`, `/workshops`, `/workshops/{slug}`, `/contact`, `/contact/thank-you` |
| **Layering** | `Controllers → Services → IWorkshopRepository`; controllers never touch data directly |
| **Validation** | Data annotations on the form model **and** independent rules in `EnquiryService`, so the rule survives a second caller |
| **Security** | Antiforgery token on the POST, parameterised SQL throughout, no raw HTML from user input |
| **Errors** | Custom 404 and 500 pages; `UseStatusCodePagesWithReExecute` keeps the real status code |
| **Front end** | Hand-written CSS, design tokens, responsive down to 360px, no Bootstrap/Tailwind/JS framework |
| **Responsive nav** | Below 720px the three header links collapse into a right-hand drawer: Esc, backdrop click, focus trap, scroll lock, and auto-close on resize |
| **Language switcher** | A second right-hand drawer for EN / CN / BM; the choice persists in `localStorage`, updates `<html lang>`, and fires an `aira:langchange` event |
| **Data** | Runs with zero setup on an in-memory repository; the SQL Server schema + procedures ship in `/database` |

Deliberately **zero NuGet packages** in the web project. Nothing to restore, nothing to break.

---

## Run it

```bash
dotnet run --project src/AiraAcademy
```

Then open the URL printed in the console (usually `http://localhost:5xxx`).

The site starts with four seeded workshops, scheduled sessions, testimonials, and a working
enquiry form. Submitted enquiries are held in memory and cleared on restart — that is the
point of the in-memory adapter, not a bug.

---

## Project layout

```
portfolio/
├─ AiraAcademy.slnx
├─ database/
│  ├─ 01-schema.sql        tables, constraints, indexes, reporting view
│  ├─ 02-seed.sql          idempotent MERGE upsert for workshops + sessions
│  └─ 03-procedures.sql    list / get / insert-enquiry / recent / report
├─ src/AiraAcademy/
│  ├─ Program.cs           composition root: DI, data protection, logging
│  ├─ Models/              domain entities + view models
│  ├─ Data/                repository interface + in-memory implementation + seed
│  ├─ Services/            WorkshopCatalog, EnquiryService
│  ├─ Controllers/         Home, Workshops, Contact
│  ├─ Views/               Razor views, shared layout, workshop card partial
│  └─ wwwroot/css/site.css hand-written stylesheet
└─ tools/prepare-offline-feed.ps1   (build helper, not part of the app)
```

---

## Design decisions, and why

**Why a repository interface at all?**
Because the interesting decision is *which* storage, and that choice should be one line in
`Program.cs`. The in-memory implementation exists so the repo runs on a clean machine with no
SQL Server, no connection string and no migration step — a reviewer can clone and run.

**Why the DataProtection key ring lives inside the app folder.**
Antiforgery tokens need a stable key ring. The default writes to the machine profile, which
breaks in containers, on read-only file systems, and under restricted service accounts. Pinning
it to `ContentRoot/.dataprotection-keys` makes the app portable.

**Why console-only logging.**
The default Windows EventLog sink throws under restricted accounts, and a logging failure
turning into a crashed request is a genuinely bad failure mode. Console output is portable and
is what a container expects anyway.

**Why not EF Core?**
Two reasons. It would have hidden the part of this project worth reading — the SQL — and the
authoring environment had no package feed available, so any ORM would have produced an app that
could not be built. Doing it with hand-written SQL keeps the whole thing honest and compiles.

**Why `<partial name="_WorkshopCard">`.**
The card appears on the home page, the catalogue and the detail page's "also running" block.
One partial, one place to change the price formatting.

---

## The database layer

`database/` contains the SQL Server implementation of the same domain:

- **`01-schema.sql`** — tables, `CHECK` constraints, a unique slug, a filtered index on
  `Enquiries(WorkshopId)`, and a clustered index on `(CreatedAt DESC, Id)` because enquiries are
  always read newest-first.
- **`02-seed.sql`** — a `MERGE` upsert, so re-running it updates instead of duplicating.
- **`03-procedures.sql`** — five procedures, including:
  - `usp_Workshop_List` uses `OUTER APPLY` to fetch the next session per workshop in one round
    trip instead of an N+1 loop.
  - `usp_Enquiry_Insert` validates in the procedure too, and captures the generated
    `UNIQUEIDENTIFIER` with an `OUTPUT` clause — `SCOPE_IDENTITY()` does not work for GUID keys.
  - `usp_Report_EnquiriesByWorkshop` demonstrates a set-based aggregate (`SUM(CASE …)`) instead
    of a cursor.
- **`vw_WorkshopOverview`** — a reporting view using `ROW_NUMBER() OVER (PARTITION BY …)`.

**Wiring it to the app** is one class:

```csharp
public sealed class SqlServerWorkshopRepository : IWorkshopRepository
{
    // Microsoft.Data.SqlClient, 4 usp_ calls, SqlDataReader mapping
}

// Program.cs
builder.Services.AddScoped<IWorkshopRepository, SqlServerWorkshopRepository>();
```

That is the entire change. Nothing in `Controllers/` or `Services/` moves.

---

## Verified behaviour

The following were exercised against a running instance:

- `GET /` `GET /workshops` `GET /workshops/{slug}` `GET /contact` → **200**
- `GET /workshops/does-not-exist` → **404** with the custom page and the correct status code
- `POST /contact` with valid data → **302** to `/contact/thank-you`, reference rendered
- `POST /contact` with a bad email and an empty name → **200** re-render with per-field messages
- `GET /css/site.css` → **200** (static files served)

---

## Roadmap

- [ ] Wire real translations to the language switcher (`aira:langchange` is already emitted)
- [ ] xUnit tests around `WorkshopCatalog` and `EnquiryService` (the interfaces are already there)
- [ ] Integration tests against LocalDB running the real procedures
- [ ] `SqlServerWorkshopRepository` added as a real project in the solution
- [ ] Minimal admin view for `usp_Enquiry_Recent`
- [ ] Dockerfile + GitHub Actions build

---

## Notes

The company, people, prices and quotes in this project are fictional. It is a portfolio
demonstration, not a real business, and the sample content deliberately avoids using any
existing brand's materials.
