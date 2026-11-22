using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using AiraAcademy.Models;

namespace AiraAcademy.Models;

public sealed record HomeViewModel(
    IReadOnlyList<Workshop> Featured,
    IReadOnlyList<Testimonial> Testimonials,
    int WorkshopCount,
    int BuilderCount,
    int CityCount);

public sealed record WorkshopDetailViewModel(
    Workshop Workshop,
    WorkshopSession? NextSession,
    IReadOnlyList<Workshop> Related);

public sealed class EnquiryForm
{
    [Required(ErrorMessage = "Please tell us your name.")]
    [StringLength(80, ErrorMessage = "That name is a little long.")]
    [Display(Name = "Your name")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "We need an email address to reply to.")]
    [EmailAddress(ErrorMessage = "That email address does not look right.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "That phone number does not look right.")]
    [StringLength(30)]
    [Display(Name = "Phone (optional)")]
    public string? Phone { get; set; }

    [Display(Name = "Which workshop?")]
    public string? WorkshopSlug { get; set; }

    [Required(ErrorMessage = "A sentence about what you need is enough.")]
    [StringLength(1200, MinimumLength = 10, ErrorMessage = "Please write at least a sentence.")]
    [Display(Name = "What are you trying to solve?")]
    public string Message { get; set; } = string.Empty;

    public IReadOnlyList<SelectListItem> WorkshopOptions { get; set; } = [];
}

public sealed record ThankYouViewModel(Guid Reference, string? WorkshopTitle);
