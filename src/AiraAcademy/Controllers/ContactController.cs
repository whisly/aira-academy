using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AiraAcademy.Models;
using AiraAcademy.Services;

namespace AiraAcademy.Controllers;

public sealed class ContactController : Controller
{
    private readonly IWorkshopCatalog _catalog;
    private readonly IEnquiryService _enquiries;

    public ContactController(IWorkshopCatalog catalog, IEnquiryService enquiries)
    {
        _catalog = catalog;
        _enquiries = enquiries;
    }

    [HttpGet("/contact")]
    public IActionResult Index(string? workshop)
    {
        var form = new EnquiryForm
        {
            WorkshopSlug = _catalog.Exists(workshop ?? string.Empty) ? workshop : null,
            WorkshopOptions = BuildOptions()
        };

        return View(form);
    }

    [HttpPost("/contact")]
    [ValidateAntiForgeryToken]
    public IActionResult Index(EnquiryForm form)
    {
        if (!ModelState.IsValid)
        {
            form.WorkshopOptions = BuildOptions();
            return View(form);
        }

        var result = _enquiries.Submit(form.Name, form.Email, form.Phone, form.WorkshopSlug, form.Message);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Something went wrong. Please try again.");
            form.WorkshopOptions = BuildOptions();
            return View(form);
        }

        return RedirectToAction(nameof(ThankYou), new { id = result.EnquiryId, workshop = form.WorkshopSlug });
    }

    [HttpGet("/contact/thank-you")]
    public IActionResult ThankYou(Guid? id)
    {
        if (id is null || id == Guid.Empty) return RedirectToAction(nameof(Index));

        var workshopTitle = _catalog.GetAll()
            .FirstOrDefault(w => w.Slug == Request.Query["workshop"])?
            .Title;

        return View(new ThankYouViewModel(id.Value, workshopTitle));
    }

    private IReadOnlyList<SelectListItem> BuildOptions()
    {
        var options = new List<SelectListItem>
        {
            new() { Value = string.Empty, Text = "Not sure yet — help me choose" }
        };

        options.AddRange(_catalog.GetAll().Select(w => new SelectListItem
        {
            Value = w.Slug,
            Text = w.Title
        }));

        return options;
    }
}
