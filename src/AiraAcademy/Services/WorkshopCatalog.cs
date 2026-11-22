using System.Globalization;
using AiraAcademy.Data;
using AiraAcademy.Models;

namespace AiraAcademy.Services;

public interface IWorkshopCatalog
{
    IReadOnlyList<Workshop> GetAll();
    IReadOnlyList<Workshop> GetFeatured();
    Workshop? Find(string slug);
    bool Exists(string slug);
    string FormatPrice(int amountMyr);
    WorkshopSession? NextSession(Workshop workshop);
}

public sealed class WorkshopCatalog : IWorkshopCatalog
{
    private readonly IWorkshopRepository _repository;

    public WorkshopCatalog(IWorkshopRepository repository) => _repository = repository;

    public IReadOnlyList<Workshop> GetAll() => _repository.GetWorkshops();

    public IReadOnlyList<Workshop> GetFeatured() =>
        _repository.GetWorkshops().Where(w => w.IsFeatured).ToList();

    public Workshop? Find(string slug) =>
        string.IsNullOrWhiteSpace(slug) ? null : _repository.GetWorkshop(slug);

    public bool Exists(string slug) => Find(slug) is not null;

    public string FormatPrice(int amountMyr) =>
        "RM" + amountMyr.ToString("N0", CultureInfo.InvariantCulture);

    public WorkshopSession? NextSession(Workshop workshop) =>
        workshop.Sessions.Where(s => s.StartsOn >= DateOnly.FromDateTime(DateTime.UtcNow))
                         .OrderBy(s => s.StartsOn)
                         .FirstOrDefault();
}
