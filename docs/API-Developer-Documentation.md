# French UBL Invoice Generator – API Developer Documentation

## Base details

- **Protocol:** HTTP/1.1
- **Content type (request):** `application/json`
- **Content type (success response):** `application/xml`
- **Error response:** JSON payload with an `error` property

## Endpoint summary

### `POST /api/invoices/generate`

Generate a UBL 2.1 invoice XML document from JSON invoice data.

#### Request body model

```json
{
  "invoiceNumber": "INV-2026-0001",
  "issueDate": "2026-01-15",
  "dueDate": "2026-02-14",
  "currencyCode": "EUR",
  "seller": {
    "legalName": "Seller Company SAS",
    "siret": "12345678901234",
    "vatIdentifier": "FR12345678901",
    "street": "10 Rue Exemple",
    "city": "Paris",
    "postalZone": "75001",
    "countryCode": "FR",
    "endpointId": "9957:12345678901234"
  },
  "buyer": {
    "legalName": "Buyer Company SARL",
    "siret": "98765432109876",
    "vatIdentifier": "FR98765432109",
    "street": "25 Avenue Client",
    "city": "Lyon",
    "postalZone": "69001",
    "countryCode": "FR",
    "endpointId": "9957:98765432109876"
  },
  "paymentMeansCode": "30",
  "paymentIban": "FR7630006000011234567890189",
  "paymentBic": "AGRIFRPP",
  "lines": [
    {
      "name": "Consulting services",
      "quantity": 10,
      "unitCode": "HUR",
      "unitPriceExcludingTax": 120.00,
      "vatPercent": 20
    }
  ]
}
```

> Notes:
> - Dates are parsed as `DateOnly` and should be ISO format `YYYY-MM-DD`.
> - Model binding is case-insensitive for JSON property names by default in ASP.NET Core.

## Field reference

### Root object (`InvoiceRequest`)

- `invoiceNumber` (`string`, required)
- `issueDate` (`date`, required)
- `dueDate` (`date`, required)
- `currencyCode` (`string`, optional; default `EUR`)
- `seller` (`Party`, required)
- `buyer` (`Party`, required)
- `paymentMeansCode` (`string`, optional; default `30`)
- `paymentIban` (`string`, optional)
- `paymentBic` (`string`, optional)
- `lines` (`InvoiceLineRequest[]`, required; min 1)

### Party object (`seller` / `buyer`)

- `legalName` (`string`, required)
- `siret` (`string`, optional in code, typically required in real flows)
- `vatIdentifier` (`string`, optional in code, typically required in real flows)
- `street` (`string`, required)
- `city` (`string`, required)
- `postalZone` (`string`, required)
- `countryCode` (`string`, required; must be 2 chars)
- `endpointId` (`string`, optional)

### Invoice line object

- `name` (`string`, recommended)
- `quantity` (`decimal`, required)
- `unitCode` (`string`, optional; default `C62`)
- `unitPriceExcludingTax` (`decimal`, required)
- `vatPercent` (`decimal`, required)

## Success response

- **Status:** `200 OK`
- **Body:** XML string for a UBL 2.1 `<Invoice>` document.

## Error responses

### Business validation failure

- **Status:** `400 Bad Request`
- **Body example:**

```json
{
  "error": "Invoice number is required."
}
```

### JSON/model binding failure

ASP.NET Core may return a framework-level `400 Bad Request` if JSON cannot be parsed or types are invalid.

## cURL examples

### Successful generation

```bash
curl -X POST "http://localhost:5000/api/invoices/generate" \
  -H "Content-Type: application/json" \
  -d @invoice-request.json
```

### Inspect output as file

```bash
curl -X POST "http://localhost:5000/api/invoices/generate" \
  -H "Content-Type: application/json" \
  -d @invoice-request.json \
  -o invoice.xml
```

## Implementation notes for developers

- Endpoint is mapped in `Program.cs` using Minimal APIs.
- Core generation logic sits in `UblInvoiceGenerator`.
- Validation currently throws `ArgumentException` and is translated to HTTP 400.
- Monetary values are rounded with `MidpointRounding.AwayFromZero` to 2 decimals.
- VAT subtotals are grouped by `vatPercent`.

## Extending the API safely

When adding new features, consider:

- Introducing versioning if you need breaking payload changes.
- Adding stronger validation (e.g., VAT ID format, IBAN/BIC format, non-negative amounts).
- Adding automated XSD/Schematron verification for generated XML.
- Documenting each new business rule in this API document.
