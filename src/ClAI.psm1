# ClAI module loader
$script:ModuleRoot = Split-Path -Parent $PSCommandPath
$script:PublicPath = Join-Path $ModuleRoot 'Public'
$script:PrivatePath = Join-Path $ModuleRoot 'Private'

# Import private helpers first
Get-ChildItem -Path $script:PrivatePath -Filter '*.ps1' | ForEach-Object { . $_.FullName }

# Import public functions
Get-ChildItem -Path $script:PublicPath -Filter '*.ps1' | ForEach-Object { . $_.FullName }

Export-ModuleMember -Function @(
    'Install-ClAI','Invoke-ClAITask','Get-ClAIConfig','Set-ClAIConfig',
    'Get-ClAIMcpServers','Add-ClAIMcpServer','Remove-ClAIMcpServer','Start-ClAIMcpServer','Stop-ClAIMcpServer'
)
