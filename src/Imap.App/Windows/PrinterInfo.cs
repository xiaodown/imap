namespace Imap.App.Windows;

public sealed record PrinterInfo(
    string Name,
    string? ServerName,
    string? PortName,
    PrinterAttributes Attributes,
    PrinterKind Kind);
