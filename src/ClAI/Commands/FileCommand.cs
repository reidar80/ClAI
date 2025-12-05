using System.CommandLine;
using System.Diagnostics;
using System.Text;
using ClAI.Models;
using ClAI.Services;

namespace ClAI.Commands;

/// <summary>
/// File management commands with AI assistance.
/// </summary>
public class FileCommand : Command
{
    public FileCommand(IAiService aiService) : base("file", "AI-assisted file management")
    {
        var searchCommand = new Command("search", "Search for files using natural language");
        var searchQueryArg = new Argument<string>("query", "What to search for (e.g., 'large pdf files from last week')");
        var searchPathOption = new Option<string?>(new[] { "--path", "-p" }, "Starting directory (defaults to current)");
        searchCommand.AddArgument(searchQueryArg);
        searchCommand.AddOption(searchPathOption);
        searchCommand.SetHandler(async (query, path) =>
        {
            await HandleSearchAsync(aiService, query, path ?? Directory.GetCurrentDirectory());
        }, searchQueryArg, searchPathOption);

        var organizeCommand = new Command("organize", "Get AI suggestions for organizing files");
        var organizePathArg = new Argument<string?>("path", () => null, "Directory to analyze");
        organizeCommand.AddArgument(organizePathArg);
        organizeCommand.SetHandler(async (path) =>
        {
            await HandleOrganizeAsync(aiService, path ?? Directory.GetCurrentDirectory());
        }, organizePathArg);

        var analyzeCommand = new Command("analyze", "Analyze file or directory content");
        var analyzePathArg = new Argument<string>("path", "File or directory to analyze");
        analyzeCommand.AddArgument(analyzePathArg);
        analyzeCommand.SetHandler(async (path) =>
        {
            await HandleAnalyzeAsync(aiService, path);
        }, analyzePathArg);

        AddCommand(searchCommand);
        AddCommand(organizeCommand);
        AddCommand(analyzeCommand);
    }

    private static async Task HandleSearchAsync(IAiService aiService, string query, string path)
    {
        Console.WriteLine("🔍 Generating search command...\n");

        var prompt = $"""
            Generate a PowerShell command to search for files matching this description: "{query}"
            Starting directory: {path}
            
            Consider:
            - File size conditions
            - File type/extension
            - Date modified/created
            - File name patterns
            - Content search if applicable
            
            Use Get-ChildItem with appropriate filters.
            """;

        var suggestion = await aiService.GetCommandSuggestionAsync(prompt);

        Console.WriteLine($"📋 Search Command:");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"   {suggestion.Command}");
        Console.ResetColor();
        Console.WriteLine();
        Console.WriteLine($"📖 {suggestion.Explanation}");
        Console.WriteLine();

        Console.Write("Execute search? (y/N): ");
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            await ExecutePowerShellAsync(suggestion.Command);
        }
    }

    private static async Task HandleOrganizeAsync(IAiService aiService, string path)
    {
        Console.WriteLine($"📁 Analyzing directory: {path}\n");

        if (!Directory.Exists(path))
        {
            Console.WriteLine($"❌ Directory not found: {path}");
            return;
        }

        // Get directory listing
        var files = Directory.GetFiles(path);
        var dirs = Directory.GetDirectories(path);

        var listing = new StringBuilder();
        listing.AppendLine($"Directory: {path}");
        listing.AppendLine($"Files ({files.Length}):");
        foreach (var file in files.Take(50)) // Limit to 50 files
        {
            var info = new FileInfo(file);
            listing.AppendLine($"  {info.Name} ({info.Length} bytes, {info.LastWriteTime:yyyy-MM-dd})");
        }
        if (files.Length > 50)
            listing.AppendLine($"  ... and {files.Length - 50} more files");

        listing.AppendLine($"\nSubdirectories ({dirs.Length}):");
        foreach (var dir in dirs.Take(20))
        {
            listing.AppendLine($"  {Path.GetFileName(dir)}/");
        }

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = $"Analyze this directory structure and suggest how to better organize it:\n\n{listing}",
            SystemContext = """
                You are a file organization expert. Analyze the directory structure and provide:
                1. Observations about the current organization
                2. Specific suggestions for improvement
                3. PowerShell commands to implement the suggestions
                
                Be practical and consider common workflows.
                """,
            Temperature = 0.5
        });

        if (response.Success)
        {
            Console.WriteLine("💡 Organization Suggestions:\n");
            Console.WriteLine(response.Content);
        }
        else
        {
            Console.WriteLine($"❌ Error: {response.Error}");
        }
    }

    private static async Task HandleAnalyzeAsync(IAiService aiService, string path)
    {
        if (File.Exists(path))
        {
            await AnalyzeFileAsync(aiService, path);
        }
        else if (Directory.Exists(path))
        {
            await AnalyzeDirectoryAsync(aiService, path);
        }
        else
        {
            Console.WriteLine($"❌ Path not found: {path}");
        }
    }

    private static async Task AnalyzeFileAsync(IAiService aiService, string filePath)
    {
        var info = new FileInfo(filePath);
        Console.WriteLine($"📄 Analyzing file: {info.Name}\n");

        var content = new StringBuilder();
        content.AppendLine($"File: {info.Name}");
        content.AppendLine($"Size: {info.Length:N0} bytes");
        content.AppendLine($"Created: {info.CreationTime}");
        content.AppendLine($"Modified: {info.LastWriteTime}");
        content.AppendLine($"Extension: {info.Extension}");

        // Try to read text content for small text files
        var textExtensions = new[] { ".txt", ".md", ".json", ".xml", ".csv", ".log", ".ps1", ".bat", ".cmd", ".cs", ".py", ".js" };
        if (textExtensions.Contains(info.Extension.ToLower()) && info.Length < 50000)
        {
            try
            {
                var fileContent = await File.ReadAllTextAsync(filePath);
                content.AppendLine($"\nContent preview (first 1000 chars):");
                content.AppendLine(fileContent.Length > 1000 ? fileContent[..1000] + "..." : fileContent);
            }
            catch
            {
                content.AppendLine("\n(Could not read file content)");
            }
        }

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = $"Analyze this file and provide insights:\n\n{content}",
            SystemContext = "You are a file analysis expert. Provide useful insights about the file including its purpose, potential uses, and any recommendations.",
            Temperature = 0.5
        });

        Console.WriteLine("📊 Analysis:\n");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task AnalyzeDirectoryAsync(IAiService aiService, string dirPath)
    {
        var info = new DirectoryInfo(dirPath);
        Console.WriteLine($"📁 Analyzing directory: {info.Name}\n");

        var files = info.GetFiles("*", SearchOption.AllDirectories);
        var dirs = info.GetDirectories("*", SearchOption.AllDirectories);

        var extensionGroups = files
            .GroupBy(f => f.Extension.ToLower())
            .Select(g => new { Extension = g.Key, Count = g.Count(), TotalSize = g.Sum(f => f.Length) })
            .OrderByDescending(x => x.TotalSize)
            .Take(10);

        var content = new StringBuilder();
        content.AppendLine($"Directory: {info.FullName}");
        content.AppendLine($"Total files: {files.Length}");
        content.AppendLine($"Total subdirectories: {dirs.Length}");
        content.AppendLine($"Total size: {files.Sum(f => f.Length):N0} bytes");
        content.AppendLine("\nTop file types by size:");
        foreach (var group in extensionGroups)
        {
            content.AppendLine($"  {(string.IsNullOrEmpty(group.Extension) ? "(no extension)" : group.Extension)}: {group.Count} files, {group.TotalSize:N0} bytes");
        }

        var response = await aiService.GetCompletionAsync(new AiRequest
        {
            Prompt = $"Analyze this directory structure and provide insights:\n\n{content}",
            SystemContext = "You are a file system analysis expert. Provide insights about the directory including its likely purpose, potential issues (large files, duplicates, etc.), and recommendations.",
            Temperature = 0.5
        });

        Console.WriteLine("📊 Analysis:\n");
        Console.WriteLine(response.Success ? response.Content : $"Error: {response.Error}");
    }

    private static async Task ExecutePowerShellAsync(string command)
    {
        Console.WriteLine("\n🚀 Executing...\n");

        try
        {
            // Use Base64-encoded command to prevent command injection
            var bytes = System.Text.Encoding.Unicode.GetBytes(command);
            var encodedCommand = Convert.ToBase64String(bytes);

            var processInfo = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -EncodedCommand {encodedCommand}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("❌ Failed to start PowerShell.");
                return;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (!string.IsNullOrWhiteSpace(output))
            {
                Console.WriteLine(output);
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
        }
    }
}
