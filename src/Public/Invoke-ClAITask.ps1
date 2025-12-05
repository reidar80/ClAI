function Invoke-ClAITask {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory, Position=0)]
        [string] $Prompt,
        [switch] $PreviewOnly,
        [switch] $AutoRunSafe,
        [switch] $VerboseOutput,
        [string] $WorkingDirectory,
        [string] $Skill
    )
    <#
    .SYNOPSIS
    Sends a natural-language task to the configured LLM and optionally executes suggested commands.
    #>

    $config = Read-ClAIConfig
    $skillContext = Get-ClAISkillContext -Skill $Skill
    $messages = Build-ClAIConversation -UserPrompt $Prompt -SkillContext $skillContext

    if ($VerboseOutput) {
        Write-Host "Sending messages:" -ForegroundColor Gray
        $messages | Format-List | Out-String | Write-Host
    }

    try {
        $llmResponse = Invoke-ClAILlm -Messages $messages -Model $config.DefaultModel
    }
    catch {
        throw "Failed to contact LLM: $($_.Exception.Message)"
    }

    $plan = $llmResponse
    $commands = @()
    if ($llmResponse -match '```') {
        $segments = [regex]::Matches($llmResponse, '```[^\n]*\n(?<code>[\s\S]*?)```')
        if ($segments.Count -gt 0) {
            $commands = $segments[0].Groups['code'].Value -split "`n" | Where-Object { $_.Trim() -ne '' }
        }
    }
    if (-not $commands -or $commands.Count -eq 0) {
        $commands = ($llmResponse -split "`n") | Where-Object { $_ -match '^\s*(\d+\.|-)' } | ForEach-Object { ($_ -replace '^\s*(\d+\.|-)\s*','').Trim() }
    }

    Write-Host "Plan:" -ForegroundColor Cyan
    Write-Host $plan

    $behavior = $config.ExecutionBehavior
    if ($PreviewOnly) { $behavior = 'PreviewOnly' }
    elseif ($AutoRunSafe) { $behavior = 'AutoRunSafe' }

    if ($commands.Count -eq 0) {
        Write-Warning 'LLM did not return any commands to execute.'
        return
    }

    Invoke-ClAIExecution -Commands $commands -WorkingDirectory $WorkingDirectory -Behavior $behavior
}
