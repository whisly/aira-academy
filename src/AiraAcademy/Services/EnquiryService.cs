using AiraAcademy.Data;
using AiraAcademy.Models;

namespace AiraAcademy.Services;

public sealed record EnquiryResult(bool Succeeded, string? Error, Guid? EnquiryId);

public interface IEnquiryService
{
    EnquiryResult Submit(string name, string email, string? phone, string? workshopSlug, string message);
    IReadOnlyList<Enquiry> Recent();
}

public sealed class EnquiryService : IEnquiryService
{
    private readonly IWorkshopRepository _repository;
    private readonly IWorkshopCatalog _catalog;

    public EnquiryService(IWorkshopRepository repository, IWorkshopCatalog catalog)
    {
        _repository = repository;
        _catalog = catalog;
    }

    public EnquiryResult Submit(string name, string email, string? phone, string? workshopSlug, string message)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new EnquiryResult(false, "Please tell us your name.", null);

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.EndsWith('@'))
            return new EnquiryResult(false, "Please give us an email address we can reply to.", null);

        if (string.IsNullOrWhiteSpace(message) || message.Trim().Length < 10)
            return new EnquiryResult(false, "A sentence or two about what you need is enough.", null);

        // A slug that does not exist in the catalogue is treated as "no preference"
        // rather than rejected, so a stale bookmark never blocks a real enquiry.
        var slug = _catalog.Exists(workshopSlug ?? string.Empty) ? workshopSlug : null;

        var enquiry = new Enquiry
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim(),
            WorkshopSlug = slug,
            Message = message.Trim()
        };

        _repository.AddEnquiry(enquiry);
        return new EnquiryResult(true, null, enquiry.Id);
    }

    public IReadOnlyList<Enquiry> Recent() => _repository.GetEnquiries();
}
