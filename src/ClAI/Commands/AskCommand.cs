using System.CommandLine;
using System.Diagnostics;
using ClAI.Services;

namespace ClAI.Commands;

/// <summary>
/// Command to ask the AI for help with a task.
/// </summary>
public class AskCommand : Command
{
    public AskCommand(IAiService aiService) : base("ask", "Ask the AI for help with a task")
    {
        var queryArgument = new Argument<string>("query", "Your question or task description");
        AddArgument(queryArgument);

        var executeOption = new Option<bool>(
            new[] { "--execute", "-e" },
            "Execute the suggested command after confirmation"
        );
        AddOption(executeOption);

        this.SetHandler(async (query, execute) =>
        {
            await HandleAsync(aiService, query, execute);
        }, queryArgument, executeOption);
    }

    private static async Task HandleAsync(IAiService aiService, string query, bool execute)
    {
        Console.WriteLine("🤖 Processing your request...\n");

        var suggestion = await aiService.GetCommandSuggestionAsync(query);

        if (string.IsNullOrEmpty(suggestion.Command))
        {
            Console.WriteLine($"❌ {suggestion.Explanation}");
            return;
        }

        Console.WriteLine($"📋 Suggested Command:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"   {suggestion.Command}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine($"📖 Explanation:");
        Console.WriteLine($"   {suggestion.Explanation}");
        Console.WriteLine();

        if (suggestion.IsDangerous)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"⚠️  Warning: {suggestion.Warning ?? "This command may be dangerous!"}");
            Console.ResetColor();
            Console.WriteLine();
        }

        if (execute)
        {
            Console.Write("Execute this command? (y/N): ");
            var response = Console.ReadLine()?.Trim().ToLower();

            if (response == "y" || response == "yes")
            {
                await ExecuteCommandAsync(suggestion.Command);
            }
            else
            {
                Console.WriteLine("Command execution cancelled.");
            }
        }
        else
        {
            Console.WriteLine("💡 Use --execute (-e) flag to run the command after confirmation.");
        }
    }

    private static async Task ExecuteCommandAsync(string command)
    {
        Console.WriteLine("\n🚀 Executing command...\n");

        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command.Replace("\"", "\\\"")}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("❌ Failed to start process.");
                return;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine("📤 Output:");
                Console.WriteLine(output);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Error:");
                Console.WriteLine(error);
                Console.ResetColor();
            }

            Console.WriteLine($"\n✅ Command completed with exit code: {process.ExitCode}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Error executing command: {ex.Message}");
            Console.ResetColor();
        }
    }
}
