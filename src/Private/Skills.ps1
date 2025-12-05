$script:SkillRegistry = @{
    'Default' = [ordered]@{
        Name = 'Default'
        Prompt = 'You have access to basic PowerShell/CLI commands. Prefer safe, idempotent operations.'
        Requirements = @()
    }
    'Azure' = [ordered]@{
        Name = 'Azure'
        Prompt = 'Assume Azure CLI may be available as "az"; prompt the operator before creating or deleting resources.'
        Requirements = @('az')
    }
    'Git' = [ordered]@{
        Name = 'Git'
        Prompt = 'Assume git is available. Prefer read-only commands like "git status" or "git diff" unless the operator confirms.'
        Requirements = @('git')
    }
}

function Get-ClAISkillContext {
    param(
        [string] $Skill
    )
    if (-not $Skill -or -not $script:SkillRegistry.ContainsKey($Skill)) {
        return $script:SkillRegistry['Default'].Prompt
    }
    return $script:SkillRegistry[$Skill].Prompt
}
