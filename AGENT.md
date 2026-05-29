# Agent Instructions for IMAP

This file is repo-local guidance for any coding agent working on IMAP:
**Intentionally Misunderstood AGPL Program**.

The user wants a deadpan Windows desktop app that makes a technical/rhetorical
point. The project is funny, but the implementation should not be sloppy.

## Core Intent

The project demonstrates that a mechanical checklist of normal software
integration patterns can be abused to make silly AGPL claims.

The app should intentionally integrate with the Windows printing subsystem using
real Windows API calls. It should then display those facts in a checklist that
resembles the style of "technical proof" used in overconfident licensing drama.

The conclusion should be clear but legally cautious:

- These integration facts exist.
- Similar facts are common in real software.
- These facts alone do not prove license contamination.
- The app does not claim Microsoft violates the AGPL.

## Implementation Constraints

Prefer:

- C#
- .NET 8 or installed .NET LTS
- .NET 10 if available
- WinForms
- Simple native Windows UI
- Readable source
- Small helper classes
- Real Win32 interop where feasible
- No external package dependencies unless strongly justified

Avoid:

- WPF unless WinForms is blocked
- Electron
- Avalonia
- WebView
- Heavy UI frameworks
- Installer work in v1
- Network dependencies
- NuGet packages for trivial helpers
- Meme styling
- Ragebait copy

## Safety Rules

The app must be read-only with respect to printers and Windows services.

Never:

- Submit a print job.
- Modify printer settings.
- Install printer drivers.
- Delete printers.
- Pause or resume printer queues.
- Change default printer.
- Restart the spooler.
- Start, stop, or reconfigure Windows Update.
- Trigger a Windows Update scan.
- Require administrator rights.

If an API path might mutate system state, do not use it. Find a read-only
alternative or mark the check as `Unsupported`.

## Failure Behavior

Missing printers, unsupported APIs, signature oddities, or unavailable services
are not application failures.

The app should degrade into clear row statuses:

- `Pass`: the check completed and observed the intended integration fact.
- `Fail`: the check should have worked but did not.
- `Unavailable`: the host lacks the relevant condition, such as no network-like
  printers.
- `Unsupported`: the check is intentionally skipped because the safe
  implementation is unavailable or too brittle.

Do not crash the app for ordinary machine variation.

## UI Tone

Keep the UI compact, boring, and tool-like.

Good:

- `Dynamic loading observed`
- `Resolved EnumPrintersW from winspool.drv`
- `No network-like printers found on this host`
- `Windows Update service status read successfully`

Bad:

- `Microsoft is cooked`
- `AGPL contamination confirmed`
- `Checkmate`
- `Your move, lawyers`

The deadpan style is part of the joke. Do not over-explain the joke inside the
app.

## Legal Wording

Do not write visible copy that says or implies:

- Microsoft is violating the AGPL.
- Windows must be open sourced.
- This app proves anything legally.
- The Bambu dispute has a definitive legal answer.

Use cautious words:

- `analog`
- `resembles`
- `demonstrates`
- `illustrates`
- `would be absurd if applied mechanically`
- `not legal advice`

## Suggested Project Shape

Use this shape unless there is a concrete reason not to:

```text
imap/
  README.md
  AGENT.md
  src/
    Imap.App/
      Imap.App.csproj
      Program.cs
      MainForm.cs
      Evidence/
      Windows/
  tests/
    Imap.Tests/
      Imap.Tests.csproj
```

Do not create unnecessary abstractions. This app is small. The code should be
obvious enough that someone can read it as part of the commentary.

## Testing Guidance

Add tests where they are cheap and useful:

- Evidence row status mapping
- Exception-to-row conversion
- Printer classification helper logic
- Plaintext report formatting

Do not over-invest in mocking every Win32 call for v1. The live Windows API
checks can be manually verified on a Windows host.

