function Set-ClAIConfig {
    [CmdletBinding()]
    param(
        [string] $Backend,
        [string] $Endpoint,
        [string] $DefaultModel,
        [ValidateSet('Conservative','Standard','Aggressive')]
        [string] $SafetyLevel,
        [ValidateSet('PreviewOnly','ConfirmEach','AutoRunSafe')]
        [string] $ExecutionBehavior,
        [string] $MCPRegistryPath,
        [switch] $DisableLogging
    )
    <#
    .SYNOPSIS
    Updates ClAI configuration values.
    #>
    $config = Read-ClAIConfig
    if ($PSBoundParameters.ContainsKey('Backend')) { $config.Backend = $Backend }
    if ($PSBoundParameters.ContainsKey('Endpoint')) { $config.Endpoint = $Endpoint }
    if ($PSBoundParameters.ContainsKey('DefaultModel')) { $config.DefaultModel = $DefaultModel }
    if ($PSBoundParameters.ContainsKey('SafetyLevel')) { $config.SafetyLevel = $SafetyLevel }
    if ($PSBoundParameters.ContainsKey('ExecutionBehavior')) { $config.ExecutionBehavior = $ExecutionBehavior }
    if ($PSBoundParameters.ContainsKey('MCPRegistryPath')) { $config.MCPRegistryPath = $MCPRegistryPath }
    if ($PSBoundParameters.ContainsKey('DisableLogging')) { $config.LoggingEnabled = $false }
    Write-ClAIConfig -Config $config
    return $config
}
