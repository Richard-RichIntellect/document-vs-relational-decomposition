namespace DocumentModel;

/// <summary>
/// A statement of fact made at inception or renewal (e.g. sums insured,
/// named locations, risk questionnaire answers).
/// </summary>
public sealed class Declaration
{
    public string DeclarationId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }

    public Declaration() { }

    public Declaration(string declarationId, string description, string value, DateOnly effectiveDate)
    {
        DeclarationId = declarationId;
        Description = description;
        Value = value;
        EffectiveDate = effectiveDate;
    }
}
