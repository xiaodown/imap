using System;

namespace Imap.App.Windows;

[Flags]
public enum PrinterAttributes : uint
{
    None = 0,
    Queued = 0x00000001,
    Direct = 0x00000002,
    Default = 0x00000004,
    Shared = 0x00000008,
    Network = 0x00000010,
    Hidden = 0x00000020,
    Local = 0x00000040,
    EnableDevq = 0x00000080,
    KeepPrintedJobs = 0x00000100,
    DoCompleteFirst = 0x00000200,
    WorkOffline = 0x00000400,
    EnableBidi = 0x00000800,
    RawOnly = 0x00001000,
    Published = 0x00002000,
    Fax = 0x00004000,
    Ts = 0x00008000,
    PushedUser = 0x00020000,
    PushedMachine = 0x00040000,
    Machine = 0x00080000,
    FriendlyName = 0x00100000,
    TsGenericDriver = 0x00200000
}
