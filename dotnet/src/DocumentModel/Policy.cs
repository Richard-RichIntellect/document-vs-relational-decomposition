namespace DocumentModel;

/// <summary>
/// The aggregate root. Everything that belongs to a policy travels with
/// it as one document rather than being scattered across separate tables.
///
/// Policy -&gt; Endorsements -&gt; Declarations is naturally hierarchical, is
/// almost always retrieved as a whole, and is updated far less often than
/// it is read. That shape maps cleanly onto a single document rather than
/// three or four normalised tables joined back together on every read.
/// </summary>
public sealed class Policy
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyHolder { get; set; } = string.Empty;
    public DateOnly InceptionDate { get; set; }
    public decimal BasePremium { get; set; }
    public List<Declaration> Declarations { get; set; } = new();
    public List<Endorsement> Endorsements { get; set; } = new();

    public Policy() { }

    public Policy(string policyId, string policyHolder, DateOnly inceptionDate, decimal basePremium)
    {
        PolicyId = policyId;
        PolicyHolder = policyHolder;
        InceptionDate = inceptionDate;
        BasePremium = basePremium;
    }

    public void AddDeclaration(Declaration declaration) => Declarations.Add(declaration);

    public void ApplyEndorsement(Endorsement endorsement) => Endorsements.Add(endorsement);

    /// <summary>
    /// The whole point of keeping endorsements with the policy: this
    /// number is trivial to compute from the document you already have,
    /// with no join required.
    /// </summary>
    public decimal CurrentPremium => BasePremium + Endorsements.Sum(e => e.PremiumAdjustment);
}
