function Show-ClAICommandPreview {
    param(
        [Parameter(Mandatory)]
        [array] $Commands
    )
    Write-Host "Commands proposed:" -ForegroundColor Cyan
    $index = 1
    foreach ($cmd in $Commands) {
        Write-Host ("[{0}] {1}" -f $index, $cmd) -ForegroundColor Yellow
        $index++
    }
}

function Invoke-ClAIExecution {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [array] $Commands,
        [string] $WorkingDirectory,
        [ValidateSet('PreviewOnly','ConfirmEach','AutoRunSafe')]
        [string] $Behavior = 'ConfirmEach'
    )

    Show-ClAICommandPreview -Commands $Commands
    if ($Behavior -eq 'PreviewOnly') {
        Write-Host "Preview only. No commands executed." -ForegroundColor Green
        return
    }

    $safeVerbs = @('dir','ls','pwd','git status','whoami')
    foreach ($cmd in $Commands) {
        $shouldRun = $false
        if ($Behavior -eq 'AutoRunSafe') {
            $safeVerbsMatch = $safeVerbs | Where-Object { $cmd.TrimStart().ToLower().StartsWith($_) }
            if ($safeVerbsMatch) { $shouldRun = $true }
        }
        if (-not $shouldRun) {
            $answer = Read-Host "Run command: $cmd ? (y/n)"
            if ($answer -notin @('y','Y','yes')) { continue }
            $shouldRun = $true
        }
        if ($shouldRun) {
            try {
                if ($WorkingDirectory) { Push-Location -LiteralPath $WorkingDirectory }
                Write-Host "Executing: $cmd" -ForegroundColor Green
                $output = Invoke-Expression $cmd
                if ($output) { $output }
                Write-ClAILog -Message "Executed: $cmd" -Level 'INFO'
            }
            catch {
                Write-Warning "Command failed: $($_.Exception.Message)"
                Write-ClAILog -Message "Failed: $cmd | $($_.Exception.Message)" -Level 'ERROR'
            }
            finally {
                if ($WorkingDirectory) { Pop-Location }
            }
        }
    }
}
