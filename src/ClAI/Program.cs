using System.CommandLine;
using ClAI.Commands;
using ClAI.Configuration;
using ClAI.Services;

namespace ClAI;

/// <summary>
/// ClAI - A Windows CLI extension for generative AI assistance.
/// </summary>
public class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Load configuration
        var settings = await ConfigCommand.LoadSettingsAsync();
        
        // Create HTTP client and AI service
        var httpClient = new HttpClient();
        var aiService = new OpenAiService(httpClient, settings);

        // Build command hierarchy
        var rootCommand = new RootCommand("ClAI - AI-powered Windows CLI assistant")
        {
            new AskCommand(aiService),
            new ExplainCommand(aiService),
            new ChatCommand(aiService),
            new FileCommand(aiService),
            new SystemCommand(aiService),
            new ConfigCommand()
        };

        // Add version option
        rootCommand.Description = """
            ClAI - AI-Powered Windows CLI Assistant
            
            Use generative AI to help with common tasks:
            • File management and organization
            • System configuration and troubleshooting
            • Command generation and explanation
            • Interactive AI chat for assistance
            
            Get started:
              clai config init     - Configure your AI provider
              clai ask "your task" - Get AI help with a task
              clai chat            - Start interactive chat
            """;

        return await rootCommand.InvokeAsync(args);
    }
}
