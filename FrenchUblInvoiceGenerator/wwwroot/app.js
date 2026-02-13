const sample = {
  invoiceNumber: "F2026-0001",
  issueDate: "2026-01-15",
  dueDate: "2026-02-14",
  currencyCode: "EUR",
  paymentMeansCode: "30",
  paymentIban: "FR7630006000011234567890189",
  paymentBic: "AGRIFRPP",
  seller: {
    legalName: "ACME France SAS",
    siret: "55210055400013",
    vatIdentifier: "FR25552100554",
    street: "10 Rue de Rivoli",
    city: "Paris",
    postalZone: "75001",
    countryCode: "FR",
    endpointId: "55210055400013"
  },
  buyer: {
    legalName: "Contoso SA",
    siret: "42882285200025",
    vatIdentifier: "FR89428822852",
    street: "22 Avenue de l'Opéra",
    city: "Paris",
    postalZone: "75002",
    countryCode: "FR",
    endpointId: "42882285200025"
  },
  lines: [
    {
      name: "Consulting Services - January",
      quantity: 1,
      unitCode: "C62",
      unitPriceExcludingTax: 1800,
      vatPercent: 20
    }
  ]
};

document.getElementById("payload").value = JSON.stringify(sample, null, 2);

document.getElementById("generate").addEventListener("click", async () => {
  const output = document.getElementById("output");
  output.textContent = "Generating...";

  try {
    const body = JSON.parse(document.getElementById("payload").value);
    const response = await fetch("/api/invoices/generate", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(body)
    });

    if (!response.ok) {
      const error = await response.json();
      output.textContent = `Error: ${error.error}`;
      return;
    }

    output.textContent = await response.text();
  } catch (error) {
    output.textContent = `Invalid JSON payload: ${error.message}`;
  }
});
