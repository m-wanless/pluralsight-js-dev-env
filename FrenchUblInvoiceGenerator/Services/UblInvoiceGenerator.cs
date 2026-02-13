using System.Globalization;
using System.Xml.Linq;
using FrenchUblInvoiceGenerator.Models;

namespace FrenchUblInvoiceGenerator.Services;

public sealed class UblInvoiceGenerator
{
    private static readonly XNamespace InvoiceNs = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2";
    private static readonly XNamespace CacNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
    private static readonly XNamespace CbcNs = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";

    public string GenerateFrenchUblInvoiceXml(InvoiceRequest request)
    {
        Validate(request);

        var lineExtensions = request.Lines.Sum(x => Round2(x.Quantity * x.UnitPriceExcludingTax));
        var taxTotal = request.Lines.Sum(x => Round2(x.Quantity * x.UnitPriceExcludingTax * (x.VatPercent / 100m)));
        var payableAmount = lineExtensions + taxTotal;

        var invoice = new XElement(InvoiceNs + "Invoice",
            new XAttribute(XNamespace.Xmlns + "cac", CacNs),
            new XAttribute(XNamespace.Xmlns + "cbc", CbcNs),
            new XElement(CbcNs + "UBLVersionID", "2.1"),
            new XElement(CbcNs + "CustomizationID", "urn:cen.eu:en16931:2017#compliant#urn:fnfe-mpe.org:ci-fr:Factur-X:1.0"),
            new XElement(CbcNs + "ProfileID", "urn:fdc:peppol.eu:2017:poacc:billing:01:1.0"),
            new XElement(CbcNs + "ID", request.InvoiceNumber),
            new XElement(CbcNs + "IssueDate", request.IssueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "DueDate", request.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(CbcNs + "InvoiceTypeCode", "380"),
            new XElement(CbcNs + "DocumentCurrencyCode", request.CurrencyCode),
            BuildSupplierParty(request.Seller),
            BuildCustomerParty(request.Buyer),
            BuildPaymentMeans(request),
            BuildTaxTotal(request, taxTotal),
            BuildMonetaryTotal(request, lineExtensions, taxTotal, payableAmount),
            request.Lines.Select((line, index) => BuildLine(index + 1, line, request.CurrencyCode))
        );

        var document = new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), invoice);
        return document.ToString();
    }

    private static void Validate(InvoiceRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
        {
            throw new ArgumentException("Invoice number is required.");
        }

        if (request.Lines.Count == 0)
        {
            throw new ArgumentException("At least one invoice line is required.");
        }

        ValidateParty("seller", request.Seller);
        ValidateParty("buyer", request.Buyer);
    }

    private static void ValidateParty(string type, Party party)
    {
        if (string.IsNullOrWhiteSpace(party.LegalName) || string.IsNullOrWhiteSpace(party.Street) ||
            string.IsNullOrWhiteSpace(party.PostalZone) || string.IsNullOrWhiteSpace(party.City))
        {
            throw new ArgumentException($"{type} legal name and address are required.");
        }

        if (party.CountryCode.Length != 2)
        {
            throw new ArgumentException($"{type} country must be ISO-3166 alpha-2.");
        }
    }

    private static XElement BuildSupplierParty(Party seller) =>
        new(CacNs + "AccountingSupplierParty",
            new XElement(CacNs + "Party",
                new XElement(CbcNs + "EndpointID", seller.EndpointId),
                new XElement(CacNs + "PartyIdentification", new XElement(CbcNs + "ID", seller.Siret)),
                new XElement(CacNs + "PartyName", new XElement(CbcNs + "Name", seller.LegalName)),
                BuildAddress(seller),
                new XElement(CacNs + "PartyTaxScheme",
                    new XElement(CbcNs + "CompanyID", seller.VatIdentifier),
                    new XElement(CacNs + "TaxScheme", new XElement(CbcNs + "ID", "VAT"))),
                new XElement(CacNs + "PartyLegalEntity", new XElement(CbcNs + "RegistrationName", seller.LegalName))));

    private static XElement BuildCustomerParty(Party buyer) =>
        new(CacNs + "AccountingCustomerParty",
            new XElement(CacNs + "Party",
                new XElement(CbcNs + "EndpointID", buyer.EndpointId),
                new XElement(CacNs + "PartyIdentification", new XElement(CbcNs + "ID", buyer.Siret)),
                new XElement(CacNs + "PartyName", new XElement(CbcNs + "Name", buyer.LegalName)),
                BuildAddress(buyer),
                new XElement(CacNs + "PartyTaxScheme",
                    new XElement(CbcNs + "CompanyID", buyer.VatIdentifier),
                    new XElement(CacNs + "TaxScheme", new XElement(CbcNs + "ID", "VAT"))),
                new XElement(CacNs + "PartyLegalEntity", new XElement(CbcNs + "RegistrationName", buyer.LegalName))));

    private static XElement BuildAddress(Party party) =>
        new(CacNs + "PostalAddress",
            new XElement(CbcNs + "StreetName", party.Street),
            new XElement(CbcNs + "CityName", party.City),
            new XElement(CbcNs + "PostalZone", party.PostalZone),
            new XElement(CacNs + "Country", new XElement(CbcNs + "IdentificationCode", party.CountryCode.ToUpperInvariant())));

    private static XElement BuildPaymentMeans(InvoiceRequest request) =>
        new(CacNs + "PaymentMeans",
            new XElement(CbcNs + "PaymentMeansCode", request.PaymentMeansCode),
            new XElement(CacNs + "PayeeFinancialAccount",
                new XElement(CbcNs + "ID", request.PaymentIban),
                new XElement(CacNs + "FinancialInstitutionBranch", new XElement(CbcNs + "ID", request.PaymentBic))));

    private static XElement BuildTaxTotal(InvoiceRequest request, decimal taxTotal)
    {
        var grouped = request.Lines
            .GroupBy(x => x.VatPercent)
            .Select(group => new XElement(CacNs + "TaxSubtotal",
                new XElement(CbcNs + "TaxableAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(group.Sum(x => x.Quantity * x.UnitPriceExcludingTax))),
                new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(group.Sum(x => x.Quantity * x.UnitPriceExcludingTax * (x.VatPercent / 100m)))),
                new XElement(CacNs + "TaxCategory",
                    new XElement(CbcNs + "ID", "S"),
                    new XElement(CbcNs + "Percent", group.Key),
                    new XElement(CacNs + "TaxScheme", new XElement(CbcNs + "ID", "VAT")))));

        return new XElement(CacNs + "TaxTotal",
            new XElement(CbcNs + "TaxAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(taxTotal)),
            grouped);
    }

    private static XElement BuildMonetaryTotal(InvoiceRequest request, decimal lineExtensions, decimal taxTotal, decimal payableAmount) =>
        new(CacNs + "LegalMonetaryTotal",
            new XElement(CbcNs + "LineExtensionAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(lineExtensions)),
            new XElement(CbcNs + "TaxExclusiveAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(lineExtensions)),
            new XElement(CbcNs + "TaxInclusiveAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(lineExtensions + taxTotal)),
            new XElement(CbcNs + "PayableAmount", new XAttribute("currencyID", request.CurrencyCode), Round2(payableAmount)));

    private static XElement BuildLine(int index, InvoiceLineRequest line, string currency) =>
        new(CacNs + "InvoiceLine",
            new XElement(CbcNs + "ID", index),
            new XElement(CbcNs + "InvoicedQuantity", new XAttribute("unitCode", line.UnitCode), line.Quantity),
            new XElement(CbcNs + "LineExtensionAmount", new XAttribute("currencyID", currency), Round2(line.Quantity * line.UnitPriceExcludingTax)),
            new XElement(CacNs + "Item",
                new XElement(CbcNs + "Name", line.Name),
                new XElement(CacNs + "ClassifiedTaxCategory",
                    new XElement(CbcNs + "ID", "S"),
                    new XElement(CbcNs + "Percent", line.VatPercent),
                    new XElement(CacNs + "TaxScheme", new XElement(CbcNs + "ID", "VAT")))),
            new XElement(CacNs + "Price",
                new XElement(CbcNs + "PriceAmount", new XAttribute("currencyID", currency), Round2(line.UnitPriceExcludingTax))));

    private static decimal Round2(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
