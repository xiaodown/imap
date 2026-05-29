using Imap.App.Windows;

namespace Imap.Tests;

[TestClass]
public sealed class PrinterClassifierTests
{
    [TestMethod]
    public void Classify_TreatsUncNameAsNetworkLike()
    {
        var kind = PrinterClassifier.Classify(@"\\print-server\queue", null, null, PrinterAttributes.None);

        Assert.AreEqual(PrinterKind.NetworkLike, kind);
    }

    [TestMethod]
    public void Classify_TreatsSharedAttributeAsSharedLike()
    {
        var kind = PrinterClassifier.Classify("Office Printer", null, null, PrinterAttributes.Shared);

        Assert.AreEqual(PrinterKind.SharedLike, kind);
    }

    [TestMethod]
    public void Classify_TreatsNamedPrinterAsLocalLikeByDefault()
    {
        var kind = PrinterClassifier.Classify("Microsoft Print to PDF", null, null, PrinterAttributes.None);

        Assert.AreEqual(PrinterKind.LocalLike, kind);
    }

    [TestMethod]
    public void Classify_TreatsKnownTcpPortAsNetworkLike()
    {
        var networkPorts = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "BRN94DDF87482B5" };

        var kind = PrinterClassifier.Classify("Brother MFC", null, "BRN94DDF87482B5", PrinterAttributes.None, networkPorts);

        Assert.AreEqual(PrinterKind.NetworkLike, kind);
    }
}
