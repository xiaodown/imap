namespace Imap.App.Windows;

public static class PrinterClassifier
{
    public static PrinterKind Classify(
        string? name,
        string? serverName,
        string? portName,
        PrinterAttributes attributes,
        IReadOnlySet<string>? networkPortNames = null)
    {
        if (!string.IsNullOrWhiteSpace(serverName) ||
            (!string.IsNullOrWhiteSpace(name) && name.StartsWith(@"\\", StringComparison.Ordinal)) ||
            IsNetworkPort(portName, networkPortNames) ||
            attributes.HasFlag(PrinterAttributes.Network))
        {
            return PrinterKind.NetworkLike;
        }

        if (attributes.HasFlag(PrinterAttributes.Shared))
        {
            return PrinterKind.SharedLike;
        }

        if (attributes.HasFlag(PrinterAttributes.Local) || !string.IsNullOrWhiteSpace(name))
        {
            return PrinterKind.LocalLike;
        }

        return PrinterKind.Unknown;
    }

    private static bool IsNetworkPort(string? portName, IReadOnlySet<string>? networkPortNames)
    {
        if (string.IsNullOrWhiteSpace(portName))
        {
            return false;
        }

        if (networkPortNames?.Contains(portName) == true)
        {
            return true;
        }

        return portName.StartsWith("IP_", StringComparison.OrdinalIgnoreCase) ||
            portName.StartsWith("WSD-", StringComparison.OrdinalIgnoreCase);
    }
}
