namespace DocumentModel;

/// <summary>
/// A change applied to a policy after inception (mid-term adjustment,
/// coverage extension, correction, etc.).
/// </summary>
public sealed class Endorsement
{
    public string EndorsementId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateOnly EffectiveDate { get; set; }
    public decimal PremiumAdjustment { get; set; }

    public Endorsement() { }

    public Endorsement(string endorsementId, string description, DateOnly effectiveDate, decimal premiumAdjustment = 0m)
    {
        EndorsementId = endorsementId;
        Description = description;
        EffectiveDate = effectiveDate;
        PremiumAdjustment = premiumAdjustment;
    }
}
