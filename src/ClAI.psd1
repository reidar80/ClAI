@{
    RootModule = 'ClAI.psm1'
    ModuleVersion = '0.1.0'
    GUID = 'b7e3a2f6-7b69-4e78-a112-9873a9d45112'
    Author = 'ClAI Project'
    CompanyName = 'ClAI'
    Copyright = '(c) 2024 ClAI'
    Description = 'Local-first PowerShell module for natural-language tasking with hardware-aware LLM selection and MCP tool integration.'
    PowerShellVersion = '5.1'
    RequiredAssemblies = @()
    ScriptsToProcess = @()
    TypesToProcess = @()
    FormatsToProcess = @()
    NestedModules = @()
    FunctionsToExport = @('Install-ClAI','Invoke-ClAITask','Get-ClAIConfig','Set-ClAIConfig','Get-ClAIMcpServers','Add-ClAIMcpServer','Remove-ClAIMcpServer','Start-ClAIMcpServer','Stop-ClAIMcpServer')
    AliasesToExport = @()
    CmdletsToExport = @()
    VariablesToExport = '*'
    PrivateData = @{ }
}
