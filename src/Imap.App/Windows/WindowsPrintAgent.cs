using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using Microsoft.Win32;

namespace Imap.App.Windows;

public sealed class WindowsPrintAgent
{
    public const string SpoolerLibraryName = "winspool.drv";
    private static readonly Guid WinTrustActionGenericVerifyV2 = new("00AAC56B-CD44-11D0-8CC2-00C04FC295EE");

    public bool Initialize()
    {
        return !string.IsNullOrWhiteSpace(SpoolerLibraryName);
    }

    public string? ResolveSpoolerPath()
    {
        var path = Path.Combine(Environment.SystemDirectory, SpoolerLibraryName);
        return File.Exists(path) ? path : null;
    }

    public bool CanLoadSpoolerLibrary()
    {
        var module = NativeMethods.LoadLibrary(SpoolerLibraryName);
        if (module == IntPtr.Zero)
        {
            return false;
        }

        NativeMethods.FreeLibrary(module);
        return true;
    }

    public IReadOnlyList<string> ResolveRequiredSymbols()
    {
        var module = NativeMethods.LoadLibrary(SpoolerLibraryName);
        if (module == IntPtr.Zero)
        {
            throw NativeMethods.LastWin32Exception();
        }

        try
        {
            var symbols = new[] { "EnumPrintersW", "OpenPrinterW" };
            var resolved = new List<string>();

            foreach (var symbol in symbols)
            {
                if (NativeMethods.GetProcAddress(module, symbol) != IntPtr.Zero)
                {
                    resolved.Add(symbol);
                }
            }

            return resolved;
        }
        finally
        {
            NativeMethods.FreeLibrary(module);
        }
    }

    public string? GetSpoolerVersion()
    {
        var path = ResolveSpoolerPath();
        if (path is null)
        {
            return null;
        }

        var version = FileVersionInfo.GetVersionInfo(path);
        return FirstNonEmpty(version.ProductVersion, version.FileVersion);
    }

    public IReadOnlyList<PrinterInfo> EnumeratePrinters()
    {
        var flags = NativeMethods.PrinterEnumFlags.Local | NativeMethods.PrinterEnumFlags.Connections;

        var firstCallSucceeded = NativeMethods.EnumPrinters(
            flags,
            null,
            2,
            IntPtr.Zero,
            0,
            out var bytesNeeded,
            out var printersReturned);

        if (firstCallSucceeded && printersReturned == 0)
        {
            return [];
        }

        var firstError = Marshal.GetLastPInvokeError();
        if (!firstCallSucceeded && firstError != NativeMethods.ErrorInsufficientBuffer)
        {
            throw new Win32Exception(firstError);
        }

        if (bytesNeeded <= 0)
        {
            return [];
        }

        var buffer = Marshal.AllocHGlobal(bytesNeeded);
        try
        {
            if (!NativeMethods.EnumPrinters(flags, null, 2, buffer, bytesNeeded, out _, out printersReturned))
            {
                throw NativeMethods.LastWin32Exception();
            }

            var networkPortNames = GetNetworkPortNames();
            var entrySize = Marshal.SizeOf<NativeMethods.PrinterInfo2>();
            var printers = new List<PrinterInfo>(printersReturned);

            for (var index = 0; index < printersReturned; index++)
            {
                var entryPointer = IntPtr.Add(buffer, index * entrySize);
                var nativeInfo = Marshal.PtrToStructure<NativeMethods.PrinterInfo2>(entryPointer);
                var name = PtrToString(nativeInfo.PrinterName) ?? "(unnamed printer)";
                var serverName = PtrToString(nativeInfo.ServerName);
                var portName = PtrToString(nativeInfo.PortName);
                var kind = PrinterClassifier.Classify(name, serverName, portName, nativeInfo.Attributes, networkPortNames);
                printers.Add(new PrinterInfo(name, serverName, portName, nativeInfo.Attributes, kind));
            }

            return printers;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public TrustObservation ObservePublisherTrust()
    {
        var path = ResolveSpoolerPath();
        if (path is null)
        {
            return new TrustObservation(false, false, "winspool.drv path could not be resolved");
        }

        var fileInfo = new NativeMethods.WinTrustFileInfo(path);
        var fileInfoPointer = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.WinTrustFileInfo>());

        try
        {
            Marshal.StructureToPtr(fileInfo, fileInfoPointer, false);
            var trustData = new NativeMethods.WinTrustData(fileInfoPointer);
            var result = NativeMethods.WinVerifyTrust(IntPtr.Zero, WinTrustActionGenericVerifyV2, ref trustData);

            trustData.StateAction = 2;
            _ = NativeMethods.WinVerifyTrust(IntPtr.Zero, WinTrustActionGenericVerifyV2, ref trustData);

            return result == 0
                ? new TrustObservation(true, true, "WinVerifyTrust reported a valid Authenticode signature for winspool.drv")
                : ObserveCatalogOrAuthenticodeTrust(path, result);
        }
        finally
        {
            Marshal.DestroyStructure<NativeMethods.WinTrustFileInfo>(fileInfoPointer);
            Marshal.FreeHGlobal(fileInfoPointer);
        }
    }

    private static TrustObservation ObserveCatalogOrAuthenticodeTrust(string path, uint winTrustResult)
    {
        var signature = GetAuthenticodeSignature(path);
        if (signature is null)
        {
            return new TrustObservation(false, false, $"WinVerifyTrust returned {DescribeTrustResult(winTrustResult)}; catalog-signature fallback was unavailable");
        }

        if (string.Equals(signature.Status, "Valid", StringComparison.OrdinalIgnoreCase))
        {
            var signer = string.IsNullOrWhiteSpace(signature.SignerSubject)
                ? "signer subject unavailable"
                : signature.SignerSubject;

            return new TrustObservation(
                true,
                true,
                $"Windows Authenticode policy reported Valid; {signer}");
        }

        return new TrustObservation(
            true,
            false,
            $"WinVerifyTrust returned {DescribeTrustResult(winTrustResult)}; Windows Authenticode policy reported {signature.Status}: {signature.StatusMessage}");
    }

    private static AuthenticodeSignatureObservation? GetAuthenticodeSignature(string path)
    {
        var script = """
            $signature = Get-AuthenticodeSignature -LiteralPath $env:IMAP_SIGNATURE_PATH
            [pscustomobject]@{
                Status = $signature.Status.ToString()
                StatusMessage = $signature.StatusMessage
                SignerSubject = if ($signature.SignerCertificate) { $signature.SignerCertificate.Subject } else { $null }
            } | ConvertTo-Json -Compress
            """;

        var processInfo = new ProcessStartInfo
        {
            FileName = ResolveWindowsPowerShellPath(),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };
        processInfo.Environment["IMAP_SIGNATURE_PATH"] = path;
        processInfo.ArgumentList.Add("-NoProfile");
        processInfo.ArgumentList.Add("-NonInteractive");
        processInfo.ArgumentList.Add("-ExecutionPolicy");
        processInfo.ArgumentList.Add("Bypass");
        processInfo.ArgumentList.Add("-EncodedCommand");
        processInfo.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(script)));

        try
        {
            using var process = Process.Start(processInfo);
            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();
            _ = process.StandardError.ReadToEnd();

            if (!process.WaitForExit(5000) || process.ExitCode != 0 || string.IsNullOrWhiteSpace(output))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AuthenticodeSignatureObservation>(
                output,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (Exception ex) when (ex is Win32Exception or InvalidOperationException or JsonException)
        {
            return null;
        }
    }

    public UpdateObservation ObserveUpdateContext()
    {
        using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var build = key?.GetValue("CurrentBuild")?.ToString();
        var ubr = key?.GetValue("UBR")?.ToString();
        var displayVersion = key?.GetValue("DisplayVersion")?.ToString();

        var parts = new[]
        {
            $"OS version {Environment.OSVersion.Version}",
            string.IsNullOrWhiteSpace(displayVersion) ? null : $"display version {displayVersion}",
            string.IsNullOrWhiteSpace(build) ? null : $"build {build}",
            string.IsNullOrWhiteSpace(ubr) ? null : $"UBR {ubr}"
        }.Where(part => part is not null);

        return new UpdateObservation(true, string.Join("; ", parts));
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }

    private static string? PtrToString(IntPtr value)
    {
        return value == IntPtr.Zero ? null : Marshal.PtrToStringUni(value);
    }

    private static IReadOnlySet<string> GetNetworkPortNames()
    {
        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddPortNames(names, @"SYSTEM\CurrentControlSet\Control\Print\Monitors\Standard TCP/IP Port\Ports");
        AddPortNames(names, @"SYSTEM\CurrentControlSet\Control\Print\Monitors\WSD Port\Ports");

        return names;
    }

    private static void AddPortNames(HashSet<string> names, string registryPath)
    {
        using var key = Registry.LocalMachine.OpenSubKey(registryPath);
        if (key is null)
        {
            return;
        }

        foreach (var subKeyName in key.GetSubKeyNames())
        {
            names.Add(subKeyName);
        }
    }

    private static string DescribeTrustResult(uint result)
    {
        return result switch
        {
            0x800B0109 => "0x800B0109 (certificate chain terminates in an untrusted root)",
            0x800B010A => "0x800B010A (certificate chain could not be built)",
            0x800B0100 => "0x800B0100 (no signature was present)",
            0x80096010 => "0x80096010 (digital signature is invalid)",
            0x80096019 => "0x80096019 (basic constraints extension not observed)",
            0x80092003 => "0x80092003 (certificate or CRL could not be found)",
            _ => $"0x{result:X8}"
        };
    }

    private static string ResolveWindowsPowerShellPath()
    {
        var systemPowerShell = Path.Combine(
            Environment.SystemDirectory,
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");

        return File.Exists(systemPowerShell) ? systemPowerShell : "powershell.exe";
    }

    private sealed record AuthenticodeSignatureObservation(
        string? Status,
        string? StatusMessage,
        string? SignerSubject);
}
