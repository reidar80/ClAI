$script:McpProcesses = @{}

function Get-ClAIMcpRegistryPath {
    $config = Read-ClAIConfig
    if ($config.MCPRegistryPath) { return $config.MCPRegistryPath }
    $default = Join-Path (Split-Path (Get-ClAIConfigPath)) 'mcp.servers.json'
    return $default
}

function Read-ClAIMcpRegistry {
    $path = Get-ClAIMcpRegistryPath
    if (-not (Test-Path -LiteralPath $path)) { return @{ servers = @() } }
    try {
        $json = Get-Content -Path $path -Raw -ErrorAction Stop
        return $json | ConvertFrom-Json -ErrorAction Stop
    }
    catch {
        Write-Warning "Unable to read MCP registry: $($_.Exception.Message). Returning empty."; return @{ servers = @() }
    }
}

function Write-ClAIMcpRegistry {
    param([Parameter(Mandatory)][hashtable] $Registry)
    $path = Get-ClAIMcpRegistryPath
    $Registry | ConvertTo-Json -Depth 6 | Set-Content -Path $path -Encoding UTF8
}

function Test-ClAINodeAvailable {
    $node = Get-Command node -ErrorAction SilentlyContinue
    $npm = Get-Command npm -ErrorAction SilentlyContinue
    return [ordered]@{ Node = [bool]$node; Npm = [bool]$npm }
}

function Get-ClAIMcpServers {
    $registry = Read-ClAIMcpRegistry
    return $registry.servers
}

function Add-ClAIMcpServer {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)][string] $Name,
        [Parameter(Mandatory)][string] $Source,
        [Parameter(Mandatory)][string] $StartCommand,
        [int] $Port,
        [bool] $AutoStart = $false
    )
    $registry = Read-ClAIMcpRegistry
    $existing = $registry.servers | Where-Object { $_.name -eq $Name }
    if ($existing) { throw "Server $Name already exists." }
    $registry.servers += @{ name=$Name; source=$Source; startCommand=$StartCommand; port=$Port; autoStart=$AutoStart }
    Write-ClAIMcpRegistry -Registry $registry
}

function Remove-ClAIMcpServer {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Name)
    $registry = Read-ClAIMcpRegistry
    $registry.servers = @($registry.servers | Where-Object { $_.name -ne $Name })
    Write-ClAIMcpRegistry -Registry $registry
}

function Start-ClAIMcpServer {
    [CmdletBinding(SupportsShouldProcess)]
    param([Parameter(Mandatory)][string] $Name)
    $registry = Read-ClAIMcpRegistry
    $server = $registry.servers | Where-Object { $_.name -eq $Name }
    if (-not $server) { throw "Server $Name not found." }
    $nodeStatus = Test-ClAINodeAvailable
    if (-not $nodeStatus.Node -or -not $nodeStatus.Npm) {
        Write-Warning 'Node.js/npm not found on PATH. Please install from https://nodejs.org before starting MCP servers.'
        return
    }
    Write-Host "About to run start command: $($server.startCommand)" -ForegroundColor Cyan
    if ($PSCmdlet.ShouldProcess($server.startCommand, 'Start MCP server')) {
        $proc = Start-Process -FilePath 'powershell' -ArgumentList "-NoProfile -Command $($server.startCommand)" -PassThru
        $script:McpProcesses[$Name] = $proc
        Write-ClAILog -Message "Started MCP server $Name (PID $($proc.Id))" -Level 'INFO'
    }
}

function Stop-ClAIMcpServer {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Name)
    if ($script:McpProcesses.ContainsKey($Name)) {
        try {
            $proc = $script:McpProcesses[$Name]
            if (-not $proc.HasExited) { $proc.Kill() }
            Write-ClAILog -Message "Stopped MCP server $Name" -Level 'INFO'
        }
        catch {
            Write-Warning "Failed to stop MCP server $Name: $($_.Exception.Message)"
        }
        $script:McpProcesses.Remove($Name) | Out-Null
    }
    else {
        Write-Warning "No running MCP process tracked for $Name."
    }
}
