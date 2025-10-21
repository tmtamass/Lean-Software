$ErrorActionPreference = 'Stop'
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
$src = Join-Path $here 'Program.cs'
$out = Join-Path $here 'LeanGenerator.exe'

Write-Host 'Compiling UI to' $out '...'
$code = Get-Content -Raw -Path $src
Add-Type -TypeDefinition $code -ReferencedAssemblies 'System.Windows.Forms','System.Drawing' -OutputType 'WindowsApplication' -OutputAssembly $out | Out-Null
Write-Host 'Done.'
