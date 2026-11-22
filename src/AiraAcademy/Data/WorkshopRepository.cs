using System.Collections.Concurrent;
using AiraAcademy.Models;

namespace AiraAcademy.Data;

public interface IWorkshopRepository
{
    IReadOnlyList<Workshop> GetWorkshops();
    Workshop? GetWorkshop(string slug);
    IReadOnlyList<Testimonial> GetTestimonials();
    void AddEnquiry(Enquiry enquiry);
    IReadOnlyList<Enquiry> GetEnquiries();
}

/// <summary>
/// In-memory implementation so the site runs with zero setup.
/// The SQL Server implementation lives in <c>database/</c> and
/// <c>src/AiraAcademy.Data.SqlServer</c> and implements the same contract.
/// </summary>
public sealed class InMemoryWorkshopRepository : IWorkshopRepository
{
    private readonly IReadOnlyList<Workshop> _workshops;
    private readonly IReadOnlyList<Testimonial> _testimonials;
    private readonly ConcurrentDictionary<Guid, Enquiry> _enquiries = new();

    public InMemoryWorkshopRepository()
    {
        _workshops = Seed.Workshops();
        _testimonials = Seed.Testimonials();
    }

    public IReadOnlyList<Workshop> GetWorkshops() => _workshops;

    public Workshop? GetWorkshop(string slug) =>
        _workshops.FirstOrDefault(w =>
            string.Equals(w.Slug, slug, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<Testimonial> GetTestimonials() => _testimonials;

    public void AddEnquiry(Enquiry enquiry) => _enquiries[enquiry.Id] = enquiry;

    public IReadOnlyList<Enquiry> GetEnquiries() =>
        _enquiries.Values.OrderByDescending(e => e.CreatedAt).ToList();
}

internal static class Seed
{
    public static IReadOnlyList<Workshop> Workshops() =>
    [
        new Workshop
        {
            Slug = "one-person-company",
            Code = "OPC-01",
            Title = "One Person Company AI Workshop",
            Summary = "Build your own website, a working sales backend and a WhatsApp auto-reply in two days.",
            Description =
                "Most AI courses leave you with notes. This one leaves you with a running business system. " +
                "You bring your product and your customers; we build the site, the order flow and the follow-up " +
                "automation together, on your own laptop, before you leave the room.",
            Audience = "Founders, freelancers and solo operators",
            Level = "Beginner friendly",
            PriceMyr = 3480,
            PriceNote = "for two people",
            DurationDays = 2,
            IsFeatured = true,
            Accent = "orange",
            Outcomes =
            [
                "A published website you control, not a template you rent",
                "An order and enquiry flow that survives real customers",
                "A WhatsApp auto-reply that handles routine questions",
                "A written handover so you can keep editing after the class"
            ],
            Outline =
            [
                "Day 1 — Map the business, design the offer, stand up the site",
                "Day 1 — Forms, validation and where the data actually goes",
                "Day 2 — Order flow, notifications, and the WhatsApp auto-reply",
                "Day 2 — Guardrails: what the AI must never answer on its own"
            ],
            Sessions =
            [
                new() { StartsOn = new DateOnly(2026, 12, 5), EndsOn = new DateOnly(2026, 12, 6), City = "Johor", SeatsLeft = 6 },
                new() { StartsOn = new DateOnly(2026, 12, 12), EndsOn = new DateOnly(2026, 12, 13), City = "Kuala Lumpur", SeatsLeft = 9 },
                new() { StartsOn = new DateOnly(2027, 1, 9), EndsOn = new DateOnly(2027, 1, 10), City = "Penang", SeatsLeft = 12 }
            ]
        },
        new Workshop
        {
            Slug = "people-ops",
            Code = "HR-01",
            Title = "People Operations AI Workshop",
            Summary = "Cut the administrative half of HR without cutting the human half.",
            Description =
                "Screening, scheduling, policy questions and onboarding paperwork all follow patterns. " +
                "This workshop shows you where AI genuinely helps, where it quietly creates risk, and how to " +
                "keep a person in the loop on anything that affects someone's job.",
            Audience = "HR managers, recruiters and office administrators",
            Level = "Beginner friendly",
            PriceMyr = 2680,
            PriceNote = "per person",
            DurationDays = 2,
            IsFeatured = true,
            Accent = "teal",
            Outcomes =
            [
                "A screening shortlist process you can defend to a candidate",
                "Reusable policy-answer drafts with sourcing built in",
                "An onboarding checklist that assembles itself",
                "A clear do-not-automate list for sensitive decisions"
            ],
            Outline =
            [
                "Day 1 — Where HR time actually goes, and what is safe to automate",
                "Day 1 — Job descriptions, screening, and bias you can measure",
                "Day 2 — Policy answers, onboarding, and internal knowledge bases",
                "Day 2 — Disclosure, consent and the audit trail"
            ],
            Sessions =
            [
                new() { StartsOn = new DateOnly(2026, 12, 8), EndsOn = new DateOnly(2026, 12, 9), City = "Kuala Lumpur", SeatsLeft = 4 },
                new() { StartsOn = new DateOnly(2027, 1, 14), EndsOn = new DateOnly(2027, 1, 15), City = "Johor", SeatsLeft = 14 }
            ]
        },
        new Workshop
        {
            Slug = "finance-operations",
            Code = "FIN-01",
            Title = "Finance Operations AI Workshop",
            Summary = "Reconcile, categorise and draft — with a reviewer still in the chair.",
            Description =
                "Finance teams do not need faster guesses; they need less manual sorting. This workshop is " +
                "built around documents you already have: invoices, statements, receipts and month-end packs. " +
                "Every workflow ends at a human approval step, on purpose.",
            Audience = "Accountants, bookkeepers and finance leads",
            Level = "Intermediate",
            PriceMyr = 2680,
            PriceNote = "per person",
            DurationDays = 2,
            IsFeatured = true,
            Accent = "plum",
            Outcomes =
            [
                "Invoice capture with a reconciliation report",
                "A variance summary you can send to a client",
                "Month-end checklist drafted from last month's actuals",
                "Controls that stop silent errors before they post"
            ],
            Outline =
            [
                "Day 1 — Document capture, extraction and confidence scoring",
                "Day 1 — Reconciliation rules and exception queues",
                "Day 2 — Client-ready summaries and variance notes",
                "Day 2 — Controls, review gates and retention"
            ],
            Sessions =
            [
                new() { StartsOn = new DateOnly(2026, 12, 15), EndsOn = new DateOnly(2026, 12, 16), City = "Penang", SeatsLeft = 7 },
                new() { StartsOn = new DateOnly(2027, 2, 4), EndsOn = new DateOnly(2027, 2, 5), City = "Kuala Lumpur", SeatsLeft = 16 }
            ]
        },
        new Workshop
        {
            Slug = "corporate-team",
            Code = "CORP-01",
            Title = "Corporate Team AI Programme",
            Summary = "One department, one real workflow, measured in weeks not quarters.",
            Description =
                "A private programme for management teams. We pick one workflow that already hurts, " +
                "map it with your people, build the automation with them, and leave behind a measured " +
                "before-and-after rather than a slide deck.",
            Audience = "Department heads and management teams",
            Level = "Leadership",
            PriceMyr = 38000,
            PriceNote = "per team, scoped",
            DurationDays = 5,
            IsFeatured = false,
            Accent = "ink",
            Outcomes =
            [
                "One named workflow, automated and measured",
                "A small internal team that can repeat the pattern",
                "A governance note your compliance team can sign",
                "Before-and-after numbers for the next budget cycle"
            ],
            Outline =
            [
                "Week 1 — Discovery, workflow mapping, baseline metrics",
                "Week 2 — Build sprints with your staff, not around them",
                "Week 3 — Hardening, access control, rollback plan",
                "Week 4 — Handover, training and measurement"
            ],
            Sessions =
            [
                new() { StartsOn = new DateOnly(2027, 1, 19), EndsOn = new DateOnly(2027, 2, 13), City = "On site", SeatsLeft = 3 }
            ]
        }
    ];

    public static IReadOnlyList<Testimonial> Testimonials() =>
    [
        new()
        {
            Quote = "We stopped answering the same twelve questions on WhatsApp. The auto-reply handles them and I only see the ones that matter.",
            Name = "Farah Idris",
            Role = "Founder, Peranakan Bakehouse",
            Initials = "FI"
        },
        new()
        {
            Quote = "The part I valued most was the list of things we should not automate. Nobody else told us that.",
            Name = "Daniel Wong",
            Role = "People Lead, Kestrel Logistics",
            Initials = "DW"
        },
        new()
        {
            Quote = "Month-end used to take four days of sorting. It now takes one afternoon plus a review.",
            Name = "Priya Raman",
            Role = "Finance Manager, Trilight Services",
            Initials = "PR"
        }
    ];
}
