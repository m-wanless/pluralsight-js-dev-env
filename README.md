# French UBL Invoice Generator (C#)

This repository now includes a C# ASP.NET Core web app that generates UBL 2.1 XML invoices with fields aligned to EN16931 + French CIUS/Factur-X oriented identifiers.

## Project location

- `FrenchUblInvoiceGenerator/`

## Run locally

```bash
cd FrenchUblInvoiceGenerator
dotnet run
```

Then open `http://localhost:5000` (or the URL shown in your terminal).

## API

- `POST /api/invoices/generate`
- Input: JSON payload matching `Models/InvoiceRequest.cs`
- Output: UBL invoice XML (`application/xml`)

## Compliance note

The generator emits structurally valid UBL 2.1 with key business fields commonly required for French e-invoicing workflows. For production-grade legal compliance, you should additionally validate generated XML against the latest official EN16931 and French CIUS schematron and XSD artifacts in your deployment pipeline.

## Tests

```bash
cd FrenchUblInvoiceGenerator.Tests
dotnet test
```

## Documentation

- App guide: `docs/App-Documentation.md`
- API developer guide: `docs/API-Developer-Documentation.md`
