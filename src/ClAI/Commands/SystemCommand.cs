using System.CommandLine;
using System.Diagnostics;
using ClAI.Models;
using ClAI.Services;

namespace ClAI.Commands;

/// <summary>
/// System settings management commands with AI assistance.
/// </summary>
public class SystemCommand : Command
{
    public SystemCommand(IAiService aiService) : base("system", "AI-assisted system management")
    {
        var infoCommand = new Command("info", "Get system information and AI insights");
        infoCommand.SetHandler(async () => await HandleInfoAsync(aiService));

        var settingCommand = new Command("setting", "Get help with a system setting");
        var settingQueryArg = new Argument<string>("query", "What setting you want to change (e.g., 'change display brightness')");
        settingCommand.AddArgument(settingQueryArg);
        settingCommand.SetHandler(async (query) => await HandleSettingAsync(aiService, query), settingQueryArg);

        var troubleshootCommand = new Command("troubleshoot", "Get AI help troubleshooting an issue");
        var issueArg = new Argument<string>("issue", "Describe the issue you're experiencing");
        troubleshootCommand.AddArgument(issueArg);
        troubleshootCommand.SetHandler(async (issue) => await HandleTroubleshootAsync(aiService, issue), issueArg);

        var processCommand = new Command("process", "Analyze running processes");
        processCommand.SetHandler(async () => await HandleProcessAsync(aiService));

        AddCommand(infoCommand);
        AddCommand(settingCommand);
        AddCommand(troubleshootCommand);
        AddCommand(processCommand);
    }

    private static async Task HandleInfoAsync(IAiService aiService)
    {
        Console.WriteLine("📊 Gathering system information...\n");

        var sysInfo = new System.Text.StringBuilder();
        sysInfo.AppendLine($"OS: {Environment.OSVersion}");
        sysInfo.AppendLine($"Machine Name: {Environment.MachineName}");
        sysInfo.AppendLine($"Processors: {Environment.ProcessorCount}");
        sysInfo.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        sysInfo.AppendLine($"CLR Version: {Environment.Version}");
        sysInfo.AppendLine($"System Directory: {Environment.SystemDirectory}");

        // Get memory info via PowerShell
        try
        {
            var memoryInfo = await RunPowerShellAsync("Get-CimInstance Win32_OperatingSystem | Select-Object TotalVisibleMemorySize, FreePhysicalMemory | Format-List");
            sysInfo.AppendLine($"\nMemory:\n{memoryInfo}");
        }
        catch
        {
            // Ignore errors on non-Windows systems
        }

        // Get disk info
        try
        {
            foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
            {
                sysInfo.AppendLine($"\nDrive {drive.Name}:");
                sysInfo.AppendLine($"  Type: {drive.DriveType}");
                sysInfo.AppendLine($"  Total: {drive.TotalSize / (1024 * 1024 * 1024):N0} GB");
                sysInfo.AppendLine($"  Free: {drive.AvailableFreeSpace / (1024 * 1024 * 1024):N0} GB");
            }
        }
        catch
        {
            // Ignore drive enumeration errors
        }

        Console.WriteLine("📋 System Information:");
        Console.WriteLine(sysInfo.ToString());

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = $"Analyze this system information and provide insights:\n\n{sysInfo}",
            SystemContext = "You are a Windows system expert. Provide brief insights about the system health, potential issues, and optimization suggestions based on the system information.",
            Temperature = 0.5
        });

        Console.WriteLine("\n💡 AI Insights:");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task HandleSettingAsync(IAiService aiService, string query)
    {
        Console.WriteLine($"🔧 Finding how to: {query}\n");

        var prompt = $"""
            The user wants to: {query}
            
            Provide step-by-step instructions for changing this Windows setting.
            Include:
            1. GUI method (Settings app or Control Panel)
            2. PowerShell/CMD command if available
            3. Any warnings or considerations
            """;

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = prompt,
            SystemContext = "You are a Windows system configuration expert. Provide clear, accurate instructions for changing system settings. Always mention if administrator privileges are required.",
            Temperature = 0.3
        });

        Console.WriteLine("📋 Instructions:");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task HandleTroubleshootAsync(IAiService aiService, string issue)
    {
        Console.WriteLine($"🔍 Troubleshooting: {issue}\n");

        // Gather recent event log errors
        string eventLogs = "";
        try
        {
            eventLogs = await RunPowerShellAsync(
                "Get-EventLog -LogName System -EntryType Error -Newest 5 2>$null | Select-Object TimeGenerated, Source, Message | Format-List"
            );
        }
        catch
        {
            // Ignore on non-Windows systems
        }

        var prompt = $"""
            User issue: {issue}
            
            Recent system errors (if available):
            {eventLogs}
            
            Provide troubleshooting steps to resolve this issue.
            """;

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = prompt,
            SystemContext = "You are a Windows troubleshooting expert. Provide systematic troubleshooting steps. Start with simple solutions before complex ones. Include PowerShell commands when helpful.",
            Temperature = 0.3
        });

        Console.WriteLine("🔧 Troubleshooting Steps:");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task HandleProcessAsync(IAiService aiService)
    {
        Console.WriteLine("📊 Analyzing running processes...\n");

        var processes = Process.GetProcesses()
            .OrderByDescending(p =>
            {
                try { return p.WorkingSet64; }
                catch { return 0; }
            })
            .Take(20)
            .Select(p =>
            {
                try
                {
                    return $"{p.ProcessName}: {p.WorkingSet64 / (1024 * 1024):N0} MB, CPU Time: {p.TotalProcessorTime.TotalSeconds:N0}s";
                }
                catch
                {
                    return $"{p.ProcessName}: (access denied)";
                }
            });

        var processList = string.Join("\n", processes);

        Console.WriteLine("Top 20 processes by memory usage:");
        Console.WriteLine(processList);
        Console.WriteLine();

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = $"Analyze these running processes and provide insights:\n\n{processList}",
            SystemContext = "You are a Windows process analysis expert. Identify any concerning processes, suggest processes that might be safely closed to free resources, and provide general observations about system resource usage.",
            Temperature = 0.5
        });

        Console.WriteLine("💡 AI Analysis:");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task<string> RunPowerShellAsync(string command)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null) return "";

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            return output;
        }
        catch
        {
            return "";
        }
    }
}
