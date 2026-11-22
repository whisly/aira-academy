namespace AiraAcademy.Models;

/// <summary>A scheduled run of a workshop in a specific city.</summary>
public sealed class WorkshopSession
{
    public required DateOnly StartsOn { get; init; }
    public required DateOnly EndsOn { get; init; }
    public required string City { get; init; }
    public int SeatsLeft { get; init; }

    public string DateRange =>
        StartsOn.Month == EndsOn.Month
            ? $"{StartsOn: d}–{EndsOn:d MMM yyyy}"
            : $"{StartsOn:d MMM} – {EndsOn:d MMM yyyy}";
}

/// <summary>A course in the catalogue.</summary>
public sealed class Workshop
{
    public required string Slug { get; init; }
    public required string Code { get; init; }
    public required string Title { get; init; }
    public required string Summary { get; init; }
    public required string Description { get; init; }
    public required string Audience { get; init; }
    public required string Level { get; init; }
    public required int PriceMyr { get; init; }
    public string PriceNote { get; init; } = string.Empty;
    public required int DurationDays { get; init; }
    public required IReadOnlyList<string> Outcomes { get; init; }
    public required IReadOnlyList<string> Outline { get; init; }
    public required IReadOnlyList<WorkshopSession> Sessions { get; init; }
    public bool IsFeatured { get; init; }
    public string Accent { get; init; } = "orange";
}

/// <summary>A short quote shown on the landing page.</summary>
public sealed class Testimonial
{
    public required string Quote { get; init; }
    public required string Name { get; init; }
    public required string Role { get; init; }
    public required string Initials { get; init; }
}

/// <summary>An enquiry submitted through the contact form.</summary>
public sealed class Enquiry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Name { get; init; }
    public required string Email { get; init; }
    public string? Phone { get; init; }
    public string? WorkshopSlug { get; init; }
    public required string Message { get; init; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
