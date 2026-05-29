using Imap.App.Evidence;

namespace Imap.Tests;

[TestClass]
public sealed class EvidenceRunnerTests
{
    [TestMethod]
    public void Run_ConvertsExceptionsToFailRowsAndContinues()
    {
        var runner = new EvidenceRunner(
        [
            new ThrowingCheck(),
            new PassingCheck()
        ]);

        var rows = runner.Run();

        Assert.HasCount(2, rows);
        Assert.AreEqual(EvidenceStatus.Fail, rows[0].Status);
        Assert.AreEqual(EvidenceStatus.Pass, rows[1].Status);
    }

    private sealed class ThrowingCheck : IEvidenceCheck
    {
        public string Claim => "Throwing claim";

        public string TechnicalAction => "Throwing action";

        public EvidenceRow Run()
        {
            throw new InvalidOperationException("test failure");
        }
    }

    private sealed class PassingCheck : IEvidenceCheck
    {
        public string Claim => "Passing claim";

        public string TechnicalAction => "Passing action";

        public EvidenceRow Run()
        {
            return new EvidenceRow(Claim, TechnicalAction, EvidenceStatus.Pass, "ok");
        }
    }
}
