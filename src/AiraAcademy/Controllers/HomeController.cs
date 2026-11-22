using Microsoft.AspNetCore.Mvc;
using AiraAcademy.Models;
using AiraAcademy.Services;

namespace AiraAcademy.Controllers;

public sealed class HomeController : Controller
{
    private readonly IWorkshopCatalog _catalog;
    private readonly Data.IWorkshopRepository _repository;

    public HomeController(IWorkshopCatalog catalog, Data.IWorkshopRepository repository)
    {
        _catalog = catalog;
        _repository = repository;
    }

    [HttpGet("/")]
    public IActionResult Index()
    {
        var model = new HomeViewModel(
            Featured: _catalog.GetFeatured(),
            Testimonials: _repository.GetTestimonials(),
            WorkshopCount: _catalog.GetAll().Count,
            BuilderCount: 640,
            CityCount: 3);

        return View(model);
    }

    [Route("/Home/Error")]
    public IActionResult Error() => View();

    [Route("/Home/NotFound")]
    public IActionResult NotFound(int? code)
    {
        ViewData["Code"] = code ?? 404;
        return View();
    }
}
