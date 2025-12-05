<#!
Convenience wrapper to run the installer from a cloned repo.
#>
$moduleRoot = Join-Path $PSScriptRoot '..' | Resolve-Path
$publicInstaller = Join-Path $moduleRoot 'src/Public/Install-ClAI.ps1'
. $publicInstaller
Install-ClAI @PSBoundParameters
