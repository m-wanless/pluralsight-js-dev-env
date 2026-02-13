# French UBL Invoice Generator – App Documentation

## Overview

The **French UBL Invoice Generator** is an ASP.NET Core web application that produces UBL 2.1 invoice XML from JSON input.

It is designed for teams that need a practical starting point for EN16931/French CIUS-aligned invoice generation workflows.

## What the app does

- Serves a browser UI for entering invoice data and generating XML.
- Exposes an API endpoint to generate invoice XML programmatically.
- Applies basic business validation before XML creation.

## Project structure

- `FrenchUblInvoiceGenerator/Program.cs` – app startup, static file hosting, and API route mapping.
- `FrenchUblInvoiceGenerator/Models/InvoiceRequest.cs` – input models (invoice, parties, lines).
- `FrenchUblInvoiceGenerator/Services/UblInvoiceGenerator.cs` – validation + UBL XML generation logic.
- `FrenchUblInvoiceGenerator/wwwroot/` – static frontend (HTML + JavaScript).

## Run the app locally

```bash
cd FrenchUblInvoiceGenerator
dotnet run
```

Open the URL printed by ASP.NET Core (commonly `http://localhost:5000`).

## Using the browser UI

1. Open the app URL in your browser.
2. Fill or paste your invoice JSON payload.
3. Submit generation.
4. Copy/download the generated UBL XML from the result panel.

## Validation behavior

The app currently validates:

- Invoice number must be provided.
- At least one invoice line is required.
- Seller and buyer must have legal name + address fields.
- Country codes must be 2 characters (ISO-3166 alpha-2 format check).

If validation fails, the API returns `400 Bad Request` with an error message.

## Output behavior

Generated XML includes major UBL invoice sections such as:

- Header metadata (`UBLVersionID`, `CustomizationID`, `ProfileID`, invoice ID/date).
- Supplier and customer party details.
- Payment means + bank account fields.
- Tax totals grouped by VAT percent.
- Legal monetary totals.
- Invoice lines with quantity, unit price, and VAT category.

## Important compliance note

This project provides a **solid technical base**, but legal e-invoicing compliance in France typically requires more than basic XML generation.

For production use, add:

- Validation against current official **EN16931 + CIUS France XSD/Schematron** packs.
- Rules for mandatory business terms specific to your sector/use case.
- Integration with your transmission channel/platform requirements.

## Testing

A separate xUnit test project is included:

```bash
cd FrenchUblInvoiceGenerator.Tests
dotnet test
```

Tests currently cover successful XML generation and core validation scenarios.
