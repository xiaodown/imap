using Imap.App.Evidence;

namespace Imap.Tests;

[TestClass]
public sealed class ReportFormatterTests
{
    [TestMethod]
    public void Format_IncludesAllRows()
    {
        var rows = new[]
        {
            new EvidenceRow("Claim one", "Action one", EvidenceStatus.Pass, "Observed one"),
            new EvidenceRow("Claim two", "Action two", EvidenceStatus.Unavailable, "Observed two")
        };

        var report = ReportFormatter.Format(rows, new DateTimeOffset(2026, 5, 29, 12, 0, 0, TimeSpan.Zero));

        StringAssert.Contains(report, "Claim one");
        StringAssert.Contains(report, "Action two");
        StringAssert.Contains(report, "[Unavailable]");
    }
}
