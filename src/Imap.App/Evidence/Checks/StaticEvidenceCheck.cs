namespace Imap.App.Evidence.Checks;

public sealed class StaticEvidenceCheck : IEvidenceCheck
{
    private readonly Func<EvidenceRow> run;

    public StaticEvidenceCheck(string claim, string technicalAction, Func<EvidenceRow> run)
    {
        Claim = claim;
        TechnicalAction = technicalAction;
        this.run = run;
    }

    public string Claim { get; }

    public string TechnicalAction { get; }

    public EvidenceRow Run()
    {
        return run();
    }
}
