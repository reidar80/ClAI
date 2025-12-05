function Install-ClAI {
    [CmdletBinding()]
    param(
        [string] $Destination
    )
    <#
    .SYNOPSIS
    Installs the ClAI module, performs hardware-aware model selection, and configures the local backend.
    #>
    $psVersion = $PSVersionTable.PSVersion
    if ($psVersion.Major -lt 5) {
        Write-Warning 'PowerShell 5.1 or later is recommended.'
    }

    # Ensure private helpers are available when the installer is run standalone
    $moduleRoot = Split-Path -Parent $PSScriptRoot
    $privatePath = Join-Path $moduleRoot 'Private'
    @('HardwareDetection.ps1', 'ModelSelection.ps1', 'Config.ps1') | ForEach-Object {
        $helperPath = Join-Path $privatePath $_
        if (Test-Path -LiteralPath $helperPath) {
            . $helperPath
        }
    }

    $runningOnWindows = $env:OS -like '*Windows*'
    if (-not $runningOnWindows) {
        Write-Warning 'Installer is optimized for Windows; continuing anyway.'
    }

    $ollama = Get-Command ollama -ErrorAction SilentlyContinue
    if (-not $ollama) {
        Write-Warning 'Ollama not found on PATH. Install from https://ollama.com/download (Windows) before pulling models.'
    }

    $hardware = Get-ClAIHardwareInfo
    $recommendation = Get-ClAIModelRecommendation -Hardware $hardware

    Write-Host 'Hardware summary:' -ForegroundColor Cyan
    $hardware | Format-List | Out-String | Write-Host
    Write-Host "Recommended model: $($recommendation.Recommendation)" -ForegroundColor Green
    Write-Host $recommendation.Reason -ForegroundColor Gray

    Write-Host 'Available models:' -ForegroundColor Cyan
    foreach ($model in $recommendation.Supported) {
        $tag = if ($model.Name -eq $recommendation.Recommendation) { ' (recommended)' } else { '' }
        Write-Host " - $($model.Display) [$($model.Name)]$tag : $($model.Notes)" -ForegroundColor Gray
    }

    $supportedNames = $recommendation.Supported.Name -join ', '
    $choice = Read-Host "Enter model to use [$($recommendation.Recommendation)] (supported: $supportedNames)"
    if (-not $choice) { $choice = $recommendation.Recommendation }

    if ($ollama) {
        $confirmPull = Read-Host "Pull model '$choice' with ollama now? (y/n)"
        if ($confirmPull -in @('y','Y','yes')) {
            try {
                Write-Host "Running: ollama pull $choice" -ForegroundColor Cyan
                ollama pull $choice
            }
            catch {
                Write-Warning "Failed to pull model: $($_.Exception.Message)"
            }
        }
    }

    $destinationPath = if ($Destination) { $Destination } else { Join-Path $([Environment]::GetFolderPath('MyDocuments')) 'PowerShell\Modules\ClAI' }
    if (-not (Test-Path -LiteralPath $destinationPath)) { New-Item -ItemType Directory -Path $destinationPath -Force | Out-Null }

    $sourcePath = Split-Path -Parent (Split-Path -Parent $PSCommandPath)
    Copy-Item -Path (Join-Path $sourcePath '*') -Destination $destinationPath -Recurse -Force

    $config = Read-ClAIConfig
    $config.DefaultModel = $choice
    $config.Backend = 'LocalOllama'
    $config.Endpoint = 'http://localhost:11434'
    $config.HardwareSnapshot = $hardware
    Write-ClAIConfig -Config $config

    Write-Host "ClAI installed to $destinationPath" -ForegroundColor Green
    Write-Host 'Import the module with: Import-Module ClAI' -ForegroundColor Green
}
