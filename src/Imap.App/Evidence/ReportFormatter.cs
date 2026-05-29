using System.Text;

namespace Imap.App.Evidence;

public static class ReportFormatter
{
    public static string Format(IReadOnlyList<EvidenceRow> rows, DateTimeOffset generatedAt)
    {
        var builder = new StringBuilder();

        builder.AppendLine("Intentionally Misunderstood AGPL Program");
        builder.AppendLine($"Generated: {generatedAt:O}");
        builder.AppendLine();

        foreach (var row in rows)
        {
            builder.AppendLine($"[{row.Status}] {row.Claim}");
            builder.AppendLine($"Action: {row.TechnicalAction}");
            builder.AppendLine($"Observed: {row.ObservedResult}");
            builder.AppendLine();
        }

        return builder.ToString();
    }
}
