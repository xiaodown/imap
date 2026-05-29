namespace Imap.App.Evidence;

public interface IEvidenceCheck
{
    string Claim { get; }
    string TechnicalAction { get; }
    EvidenceRow Run();
}
