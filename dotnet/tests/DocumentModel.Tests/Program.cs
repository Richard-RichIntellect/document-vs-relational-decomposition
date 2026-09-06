// A dependency-free test runner. No xUnit/NuGet packages required — this
// project restores and builds fully offline, same as the library and demo
// it's testing. Each test is a small function returning true/false; failures
// are printed and the process exits non-zero so it plugs into CI as-is.
// Run the project to see the test results.

using DocumentModel;

var failures = 0;
var passed = 0;

void Check(string name, bool condition)
{
    if (condition)
    {
        Console.WriteLine($"  [PASS] {name}");
        passed++;
    }
    else
    {
        Console.WriteLine($"  [FAIL] {name}");
        failures++;
    }
}

Console.WriteLine("DocumentModel.Tests");
Console.WriteLine("--------------------");

// current premium includes endorsements
{
    var policy = new Policy("POL-1", "Test Ltd", new DateOnly(2026, 1, 1), 1000.0m);
    policy.ApplyEndorsement(new Endorsement("END-1", "adjustment", new DateOnly(2026, 2, 1), 250.0m));
    policy.ApplyEndorsement(new Endorsement("END-2", "adjustment", new DateOnly(2026, 3, 1), -100.0m));
    Check("current premium includes endorsements", policy.CurrentPremium == 1150.0m);
}

// round trip through the document store (JSON serialise/deserialise)
{
    var tmpPath = Path.Combine(Path.GetTempPath(), $"_test_store_{Guid.NewGuid():N}.json");
    try
    {
        var store = new DocumentStore(tmpPath);
        var policy = new Policy("POL-2", "Roundtrip Ltd", new DateOnly(2026, 1, 1), 500.0m);
        policy.AddDeclaration(new Declaration("DEC-1", "sum insured", "£100,000", new DateOnly(2026, 1, 1)));
        policy.ApplyEndorsement(new Endorsement("END-1", "extension", new DateOnly(2026, 5, 1), 50.0m));
        store.Save(policy);

        var reloadedStore = new DocumentStore(tmpPath); // fresh instance = fresh read from disk
        var rebuilt = reloadedStore.Get("POL-2");

        Check("round trip preserves policy id", rebuilt?.PolicyId == policy.PolicyId);
        Check("round trip preserves declarations", rebuilt?.Declarations.Count == 1);
        Check("round trip preserves endorsements", rebuilt?.Endorsements.Count == 1);
        Check("round trip preserves current premium", rebuilt?.CurrentPremium == policy.CurrentPremium);
    }
    finally
    {
        if (File.Exists(tmpPath)) File.Delete(tmpPath);
    }
}

// document store persists the whole aggregate across instances
{
    var tmpPath = Path.Combine(Path.GetTempPath(), $"_test_store_{Guid.NewGuid():N}.json");
    try
    {
        var store = new DocumentStore(tmpPath);
        var policy = new Policy("POL-3", "Store Ltd", new DateOnly(2026, 1, 1), 750.0m);
        policy.AddDeclaration(new Declaration("DEC-1", "location", "London", new DateOnly(2026, 1, 1)));
        store.Save(policy);

        var reloadedStore = new DocumentStore(tmpPath);
        var reloaded = reloadedStore.Get("POL-3");

        Check("store persists across instances", reloaded is not null);
        Check("store persists policyholder", reloaded?.PolicyHolder == "Store Ltd");
        Check("store persists declarations", reloaded?.Declarations.Count == 1);
    }
    finally
    {
        if (File.Exists(tmpPath)) File.Delete(tmpPath);
    }
}

Console.WriteLine("--------------------");
Console.WriteLine($"{passed} passed, {failures} failed");

return failures == 0 ? 0 : 1;
