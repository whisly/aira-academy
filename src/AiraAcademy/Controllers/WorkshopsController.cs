using Microsoft.AspNetCore.Mvc;
using AiraAcademy.Models;
using AiraAcademy.Services;

namespace AiraAcademy.Controllers;

public sealed class WorkshopsController : Controller
{
    private readonly IWorkshopCatalog _catalog;

    public WorkshopsController(IWorkshopCatalog catalog) => _catalog = catalog;

    [HttpGet("/workshops")]
    public IActionResult Index() => View(_catalog.GetAll());

    [HttpGet("/workshops/{slug}")]
    public IActionResult Details(string slug)
    {
        var workshop = _catalog.Find(slug);
        if (workshop is null) return NotFound();

        var related = _catalog.GetAll()
            .Where(w => !string.Equals(w.Slug, slug, StringComparison.OrdinalIgnoreCase))
            .Take(2)
            .ToList();

        return View(new WorkshopDetailViewModel(workshop, _catalog.NextSession(workshop), related));
    }
}
