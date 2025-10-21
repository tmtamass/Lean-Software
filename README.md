**Lean Generator UI (Mock)**

- UI-only Windows desktop app built with WinForms
- Targets: FIVEM, STEAM, DISCORD selector (no real generation)
- Big "Generate" button shows an informational message

Build (PowerShell):

- Run: `./build.ps1`
- Output: `LeanGenerator.exe`

Run:

- Double-click `LeanGenerator.exe`

Notes:

- No external dependencies; compiles via PowerShell's C# compiler (`Add-Type`).
- If your system lacks the required .NET assemblies, install .NET Framework/Developer Pack.
