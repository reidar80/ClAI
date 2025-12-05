function Get-ClAISystemPrompt {
    param(
        [string] $SkillContext
    )
@"
You are ClAI, a Windows-focused CLI copilot. Respond with a concise plan and a list of commands. Prefer PowerShell. Avoid destructive actions unless explicitly warned. When suggesting commands, separate the plan and commands. Include safety notes. If MCP tools are available, describe how to use them but do not assume they are running.
$SkillContext
"@
}

function Build-ClAIConversation {
    param(
        [Parameter(Mandatory)]
        [string] $UserPrompt,
        [string] $SkillContext
    )

    $system = Get-ClAISystemPrompt -SkillContext $SkillContext
    return @(
        @{ role = 'system'; content = $system },
        @{ role = 'user'; content = $UserPrompt }
    )
}
