using Imap.App.Windows;

namespace Imap.App.Evidence.Checks;

public static class EvidenceCheckFactory
{
    public static IReadOnlyList<IEvidenceCheck> Create(WindowsPrintAgent agent, bool startupInitialized)
    {
        return
        [
            Check(
                "Specific proprietary subsystem named by AGPL code",
                "References winspool.drv by exact library name",
                () =>
                {
                    var path = agent.ResolveSpoolerPath();
                    var observed = path is null
                        ? $"Using subsystem identifier {WindowsPrintAgent.SpoolerLibraryName}; system path unavailable"
                        : $"Using subsystem identifier {WindowsPrintAgent.SpoolerLibraryName}; resolved path: {path}";

                    return Row(EvidenceStatus.Pass, observed);
                }),

            Check(
                "Dynamically loads proprietary subsystem",
                "Calls LoadLibraryW(\"winspool.drv\")",
                () => agent.CanLoadSpoolerLibrary()
                    ? Row(EvidenceStatus.Pass, "LoadLibraryW returned a module handle")
                    : Row(EvidenceStatus.Fail, "LoadLibraryW did not return a module handle")),

            Check(
                "Resolves closed-library function symbols",
                "Uses GetProcAddress for EnumPrintersW and another print API",
                () =>
                {
                    var symbols = agent.ResolveRequiredSymbols();
                    return symbols.Count >= 2
                        ? Row(EvidenceStatus.Pass, $"Resolved {string.Join(", ", symbols)}")
                        : Row(EvidenceStatus.Fail, $"Resolved {symbols.Count} required symbol(s): {string.Join(", ", symbols)}");
                }),

            Check(
                "Defines shared ABI for proprietary subsystem",
                "Declares managed bindings for Win32 print APIs",
                () =>
                {
                    _ = agent.EnumeratePrinters();
                    return Row(EvidenceStatus.Pass, "Managed P/Invoke bindings compiled and were used for EnumPrintersW");
                }),

            Check(
                "Defines data structures used by proprietary subsystem",
                "Parses printer information structures returned by EnumPrintersW",
                () =>
                {
                    var printers = agent.EnumeratePrinters();
                    return Row(EvidenceStatus.Pass, $"Parsed {printers.Count} PRINTER_INFO_2 record(s)");
                }),

            Check(
                "Wraps subsystem in central application manager",
                "Creates WindowsPrintAgent during application startup",
                () => startupInitialized
                    ? Row(EvidenceStatus.Pass, "WindowsPrintAgent initialized before evidence checks")
                    : Row(EvidenceStatus.Fail, "WindowsPrintAgent was not initialized before evidence checks")),

            Check(
                "Checks proprietary subsystem version",
                "Reads file version metadata for winspool.drv",
                () =>
                {
                    var version = agent.GetSpoolerVersion();
                    return version is null
                        ? Row(EvidenceStatus.Unavailable, "winspool.drv path or version metadata was unavailable")
                        : Row(EvidenceStatus.Pass, $"winspool.drv version: {version}");
                }),

            Check(
                "Uses subsystem for optional application functionality",
                "Enumerates installed printers and classifies them",
                () =>
                {
                    var printers = agent.EnumeratePrinters();
                    var network = printers.Count(printer => printer.Kind == PrinterKind.NetworkLike);
                    var local = printers.Count(printer => printer.Kind == PrinterKind.LocalLike);
                    var shared = printers.Count(printer => printer.Kind == PrinterKind.SharedLike);
                    var unknown = printers.Count(printer => printer.Kind == PrinterKind.Unknown);
                    return Row(EvidenceStatus.Pass, $"{printers.Count} printer(s) found; {network} network-like; {local} local-like; {shared} shared-like; {unknown} unknown");
                }),

            Check(
                "Network print-adjacent behavior depends on subsystem",
                "Reports network-like printer metadata when present",
                () =>
                {
                    var networkPrinters = agent.EnumeratePrinters()
                        .Where(printer => printer.Kind == PrinterKind.NetworkLike)
                        .Select(printer => printer.PortName is null ? printer.Name : $"{printer.Name} on {printer.PortName}")
                        .ToList();

                    return networkPrinters.Count > 0
                        ? Row(EvidenceStatus.Pass, $"Network-like printer(s): {string.Join(", ", networkPrinters)}")
                        : Row(EvidenceStatus.Unavailable, "No network-like printers were reported by this host");
                }),

            Check(
                "Performs publisher validation for loaded component",
                "Attempts Authenticode or trust observation for Windows printing binary",
                () =>
                {
                    var observation = agent.ObservePublisherTrust();
                    if (observation.Observed)
                    {
                        return Row(EvidenceStatus.Pass, observation.Description);
                    }

                    return Row(observation.Supported ? EvidenceStatus.Fail : EvidenceStatus.Unsupported, observation.Description);
                }),

            Check(
                "Observes update-coupled subsystem context",
                "Reads Windows Update service status or OS build information",
                () =>
                {
                    var observation = agent.ObserveUpdateContext();
                    return observation.Observed
                        ? Row(EvidenceStatus.Pass, observation.Description)
                        : Row(EvidenceStatus.Unavailable, observation.Description);
                })
        ];
    }

    private static IEvidenceCheck Check(string claim, string action, Func<EvidenceRow> run)
    {
        return new StaticEvidenceCheck(claim, action, () => run() with { Claim = claim, TechnicalAction = action });
    }

    private static EvidenceRow Row(EvidenceStatus status, string observedResult)
    {
        return new EvidenceRow(string.Empty, string.Empty, status, observedResult);
    }
}
