namespace Imap.App.Evidence;

public sealed record EvidenceRow(
    string Claim,
    string TechnicalAction,
    EvidenceStatus Status,
    string ObservedResult);
