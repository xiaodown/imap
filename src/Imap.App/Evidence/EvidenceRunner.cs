namespace Imap.App.Evidence;

public sealed class EvidenceRunner
{
    private readonly IReadOnlyList<IEvidenceCheck> checks;

    public EvidenceRunner(IEnumerable<IEvidenceCheck> checks)
    {
        this.checks = checks.ToList();
    }

    public IReadOnlyList<EvidenceRow> Run()
    {
        var rows = new List<EvidenceRow>();

        foreach (var check in checks)
        {
            try
            {
                rows.Add(check.Run());
            }
            catch (Exception ex)
            {
                rows.Add(new EvidenceRow(
                    check.Claim,
                    check.TechnicalAction,
                    EvidenceStatus.Fail,
                    $"{ex.GetType().Name}: {ex.Message}"));
            }
        }

        return rows;
    }
}
