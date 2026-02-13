namespace FrenchUblInvoiceGenerator.Models;

public sealed class InvoiceRequest
{
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateOnly IssueDate { get; init; }
    public DateOnly DueDate { get; init; }
    public string CurrencyCode { get; init; } = "EUR";
    public Party Seller { get; init; } = new();
    public Party Buyer { get; init; } = new();
    public string PaymentMeansCode { get; init; } = "30";
    public string PaymentIban { get; init; } = string.Empty;
    public string PaymentBic { get; init; } = string.Empty;
    public List<InvoiceLineRequest> Lines { get; init; } = [];
}

public sealed class Party
{
    public string LegalName { get; init; } = string.Empty;
    public string Siret { get; init; } = string.Empty;
    public string VatIdentifier { get; init; } = string.Empty;
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalZone { get; init; } = string.Empty;
    public string CountryCode { get; init; } = "FR";
    public string EndpointId { get; init; } = string.Empty;
}

public sealed class InvoiceLineRequest
{
    public string Name { get; init; } = string.Empty;
    public decimal Quantity { get; init; }
    public string UnitCode { get; init; } = "C62";
    public decimal UnitPriceExcludingTax { get; init; }
    public decimal VatPercent { get; init; }
}
