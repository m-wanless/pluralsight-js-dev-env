using System.Xml.Linq;
using FrenchUblInvoiceGenerator.Models;
using FrenchUblInvoiceGenerator.Services;

namespace FrenchUblInvoiceGenerator.Tests;

public sealed class UblInvoiceGeneratorTests
{
    private readonly UblInvoiceGenerator _generator = new();

    [Fact]
    public void GenerateFrenchUblInvoiceXml_WithValidPayload_ProducesExpectedHeaderAndTotals()
    {
        var request = BuildValidRequest();

        var xml = _generator.GenerateFrenchUblInvoiceXml(request);
        var document = XDocument.Parse(xml);

        var cbc = XNamespace.Get("urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2");

        Assert.Equal("2.1", document.Root?.Element(cbc + "UBLVersionID")?.Value);
        Assert.Equal("INV-2026-0001", document.Root?.Element(cbc + "ID")?.Value);
        Assert.Equal("EUR", document.Root?.Element(cbc + "DocumentCurrencyCode")?.Value);
        Assert.Contains("<cbc:TaxInclusiveAmount currencyID=\"EUR\">180.00</cbc:TaxInclusiveAmount>", xml);
        Assert.Contains("<cbc:PayableAmount currencyID=\"EUR\">180.00</cbc:PayableAmount>", xml);
    }

    [Fact]
    public void GenerateFrenchUblInvoiceXml_WhenInvoiceNumberMissing_ThrowsArgumentException()
    {
        var request = BuildValidRequest();
        request = new InvoiceRequest
        {
            InvoiceNumber = "",
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            CurrencyCode = request.CurrencyCode,
            Seller = request.Seller,
            Buyer = request.Buyer,
            PaymentMeansCode = request.PaymentMeansCode,
            PaymentIban = request.PaymentIban,
            PaymentBic = request.PaymentBic,
            Lines = request.Lines
        };

        var exception = Assert.Throws<ArgumentException>(() => _generator.GenerateFrenchUblInvoiceXml(request));

        Assert.Equal("Invoice number is required.", exception.Message);
    }

    [Fact]
    public void GenerateFrenchUblInvoiceXml_WhenNoLines_ThrowsArgumentException()
    {
        var request = BuildValidRequest();
        request = new InvoiceRequest
        {
            InvoiceNumber = request.InvoiceNumber,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            CurrencyCode = request.CurrencyCode,
            Seller = request.Seller,
            Buyer = request.Buyer,
            PaymentMeansCode = request.PaymentMeansCode,
            PaymentIban = request.PaymentIban,
            PaymentBic = request.PaymentBic,
            Lines = []
        };

        var exception = Assert.Throws<ArgumentException>(() => _generator.GenerateFrenchUblInvoiceXml(request));

        Assert.Equal("At least one invoice line is required.", exception.Message);
    }

    [Fact]
    public void GenerateFrenchUblInvoiceXml_WhenSellerCountryCodeNotIsoAlpha2_ThrowsArgumentException()
    {
        var request = BuildValidRequest();
        var invalidSeller = new Party
        {
            LegalName = request.Seller.LegalName,
            Siret = request.Seller.Siret,
            VatIdentifier = request.Seller.VatIdentifier,
            Street = request.Seller.Street,
            City = request.Seller.City,
            PostalZone = request.Seller.PostalZone,
            CountryCode = "FRA",
            EndpointId = request.Seller.EndpointId
        };

        request = new InvoiceRequest
        {
            InvoiceNumber = request.InvoiceNumber,
            IssueDate = request.IssueDate,
            DueDate = request.DueDate,
            CurrencyCode = request.CurrencyCode,
            Seller = invalidSeller,
            Buyer = request.Buyer,
            PaymentMeansCode = request.PaymentMeansCode,
            PaymentIban = request.PaymentIban,
            PaymentBic = request.PaymentBic,
            Lines = request.Lines
        };

        var exception = Assert.Throws<ArgumentException>(() => _generator.GenerateFrenchUblInvoiceXml(request));

        Assert.Equal("seller country must be ISO-3166 alpha-2.", exception.Message);
    }

    private static InvoiceRequest BuildValidRequest() => new()
    {
        InvoiceNumber = "INV-2026-0001",
        IssueDate = new DateOnly(2026, 1, 15),
        DueDate = new DateOnly(2026, 2, 15),
        CurrencyCode = "EUR",
        PaymentMeansCode = "30",
        PaymentIban = "FR7630006000011234567890189",
        PaymentBic = "AGRIFRPP",
        Seller = BuildValidParty("ACME France SAS", "55210055400013", "FR25552100554", "10 Rue de Rivoli", "75001"),
        Buyer = BuildValidParty("Client SAS", "42882285200025", "FR89428822852", "22 Avenue de l'Opéra", "75002"),
        Lines =
        [
            new InvoiceLineRequest
            {
                Name = "Consulting",
                Quantity = 1m,
                UnitCode = "C62",
                UnitPriceExcludingTax = 150m,
                VatPercent = 20m
            }
        ]
    };

    private static Party BuildValidParty(string legalName, string siret, string vat, string street, string postalZone) => new()
    {
        LegalName = legalName,
        Siret = siret,
        VatIdentifier = vat,
        Street = street,
        City = "Paris",
        PostalZone = postalZone,
        CountryCode = "FR",
        EndpointId = siret
    };
}
