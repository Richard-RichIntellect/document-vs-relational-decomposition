using System.Text.Json;

namespace DocumentModel;

/// <summary>
/// A minimal stand-in for a real document database (Cosmos DB, MongoDB, a
/// Postgres JSONB column, ...). It stores one JSON document per policy and
/// retrieves the whole aggregate in a single read — no joins.
///
/// Swap this class out for a real document DB client (e.g. the Cosmos DB
/// SDK) in production; the <see cref="Policy"/> model doesn't change
/// either way, which is the point.
/// </summary>
public sealed class DocumentStore
{
    private readonly string _path;
    private Dictionary<string, Policy> _data;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
    };

    public DocumentStore(string path)
    {
        _path = path;
        _data = Load(path);
    }

    private static Dictionary<string, Policy> Load(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, Policy>();
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Dictionary<string, Policy>>(json, JsonOptions)
                   ?? new Dictionary<string, Policy>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, Policy>();
        }
    }

    public void Save(Policy policy)
    {
        _data[policy.PolicyId] = policy;
        var json = JsonSerializer.Serialize(_data, JsonOptions);
        File.WriteAllText(_path, json);
    }

    public Policy? Get(string policyId) => _data.TryGetValue(policyId, out var policy) ? policy : null;

    public IReadOnlyCollection<string> AllIds() => _data.Keys.ToList();
}
