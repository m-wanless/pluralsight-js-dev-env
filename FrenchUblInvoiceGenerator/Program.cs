using FrenchUblInvoiceGenerator.Models;
using FrenchUblInvoiceGenerator.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<UblInvoiceGenerator>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/api/invoices/generate", (InvoiceRequest request, UblInvoiceGenerator generator) =>
{
    try
    {
        var xml = generator.GenerateFrenchUblInvoiceXml(request);
        return Results.Text(xml, "application/xml");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
});

app.Run();
