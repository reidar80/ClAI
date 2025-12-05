# Example usage of ClAI
Import-Module ClAI -Force

# Preview commands for a git cleanup task
Invoke-ClAITask -Prompt "Review git status and propose cleanup steps" -Skill Git -PreviewOnly

# Register an MCP server (requires node/npm installed)
Add-ClAIMcpServer -Name "example" -Source "npm:@org/example-mcp" -StartCommand "npx @org/example-mcp" -Port 8080
Start-ClAIMcpServer -Name "example" -Confirm
