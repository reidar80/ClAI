# Internal configuration helpers
$script:ConfigRoot = if ($env:USERPROFILE) { Join-Path $env:USERPROFILE 'AppData\Local\ClAI' } else { Join-Path $HOME '.clai' }
$script:ConfigPath = Join-Path $script:ConfigRoot 'config.json'
$script:LogPath = Join-Path $script:ConfigRoot 'clai.log'

function Initialize-ClAIStorage {
    if (-not (Test-Path -LiteralPath $script:ConfigRoot)) {
        New-Item -ItemType Directory -Path $script:ConfigRoot -Force | Out-Null
    }
}

function Get-ClAIConfigPath {
    Initialize-ClAIStorage
    return $script:ConfigPath
}

function Get-ClAILogPath {
    Initialize-ClAIStorage
    return $script:LogPath
}

function Read-ClAIConfig {
    Initialize-ClAIStorage
    if (-not (Test-Path -LiteralPath $script:ConfigPath)) {
        return [ordered]@{
            Backend            = 'LocalOllama'
            Endpoint           = 'http://localhost:11434'
            DefaultModel       = 'phi3-mini'
            SafetyLevel        = 'Standard'
            ExecutionBehavior  = 'PreviewOnly'
            MCPRegistryPath    = (Join-Path $script:ConfigRoot 'mcp.servers.json')
            HardwareSnapshot   = $null
            LoggingEnabled     = $true
        }
    }
    try {
        $json = Get-Content -Path $script:ConfigPath -Raw -ErrorAction Stop
        return $json | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        Write-Warning "Unable to read configuration: $($_.Exception.Message). Returning defaults."
        return [ordered]@{
            Backend            = 'LocalOllama'
            Endpoint           = 'http://localhost:11434'
            DefaultModel       = 'phi3-mini'
            SafetyLevel        = 'Standard'
            ExecutionBehavior  = 'PreviewOnly'
            MCPRegistryPath    = (Join-Path $script:ConfigRoot 'mcp.servers.json')
            HardwareSnapshot   = $null
            LoggingEnabled     = $true
        }
    }
}

function Write-ClAIConfig {
    param(
        [Parameter(Mandatory)]
        [hashtable] $Config
    )
    Initialize-ClAIStorage
    $json = $Config | ConvertTo-Json -Depth 6
    Set-Content -Path $script:ConfigPath -Value $json -Encoding UTF8
}

function Write-ClAILog {
    param(
        [string] $Message,
        [ValidateSet('INFO','WARN','ERROR')]
        [string] $Level = 'INFO'
    )
    Initialize-ClAIStorage
    $timestamp = Get-Date -Format 's'
    $entry = "[$timestamp] [$Level] $Message"
    Add-Content -Path $script:LogPath -Value $entry -Encoding UTF8
}
