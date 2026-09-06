using DocumentModel;

var storePath = Path.Combine(AppContext.BaseDirectory, "_demo_store.json");
if (File.Exists(storePath)) File.Delete(storePath);

var store = new DocumentStore(storePath);

var policy = new Policy(
    policyId: "POL-2026-00042",
    policyHolder: "Northgate Logistics Ltd",
    inceptionDate: new DateOnly(2026, 1, 1),
    basePremium: 18_500.00m);

policy.AddDeclaration(new Declaration(
    declarationId: "DEC-1",
    description: "Total insured value of warehouse contents",
    value: "£2,400,000",
    effectiveDate: new DateOnly(2026, 1, 1)));

policy.ApplyEndorsement(new Endorsement(
    endorsementId: "END-1",
    description: "Add second warehouse location (Leeds)",
    effectiveDate: new DateOnly(2026, 4, 15),
    premiumAdjustment: 3_250.00m));

store.Save(policy);
Console.WriteLine($"Saved policy {policy.PolicyId} as one document.\n");

// --- The payoff: read the whole aggregate back in a single lookup ---
var reloaded = store.Get(policy.PolicyId)!;
Console.WriteLine("Read back in a single document fetch:");
Console.WriteLine($"  Policyholder     : {reloaded.PolicyHolder}");
Console.WriteLine($"  Declarations     : {reloaded.Declarations.Count}");
Console.WriteLine($"  Endorsements     : {reloaded.Endorsements.Count}");
Console.WriteLine($"  Current premium  : £{reloaded.CurrentPremium:N2}");
Console.WriteLine();
Console.WriteLine("Compare that to sql/relational_equivalent.sql, where the same");
Console.WriteLine("read needs a policies/declarations/endorsements join and the");
Console.WriteLine("application has to de-duplicate the fan-out itself.");

File.Delete(storePath);
