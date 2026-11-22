using Microsoft.AspNetCore.DataProtection;
using AiraAcademy.Data;
using AiraAcademy.Services;

var builder = WebApplication.CreateBuilder(args);

// Console only: the default Windows EventLog sink throws under restricted
// accounts, which turns a logging call into a crashed request.
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddControllersWithViews();

// Keep the antiforgery key ring inside the app folder. This makes the app
// portable (containers, read-only user profiles) and keeps local dev free of
// machine-level state.
var keyRingPath = Path.Combine(builder.Environment.ContentRootPath, ".dataprotection-keys");
Directory.CreateDirectory(keyRingPath);
builder.Services
    .AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
    .SetApplicationName("AiraAcademy");

// Data + domain services. The repository is a singleton because the in-memory
// implementation owns the seeded catalogue and the enquiry list.
builder.Services.AddSingleton<IWorkshopRepository, InMemoryWorkshopRepository>();
builder.Services.AddSingleton<IWorkshopCatalog, WorkshopCatalog>();
builder.Services.AddSingleton<IEnquiryService, EnquiryService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/Home/NotFound", "?code={0}");
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
