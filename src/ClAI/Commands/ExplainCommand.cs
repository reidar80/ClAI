using System.CommandLine;
using ClAI.Services;

namespace ClAI.Commands;

/// <summary>
/// Command to explain a command or error message.
/// </summary>
public class ExplainCommand : Command
{
    public ExplainCommand(IAiService aiService) : base("explain", "Explain a command or error message")
    {
        var contentArgument = new Argument<string>("content", "The command or error message to explain");
        AddArgument(contentArgument);

        this.SetHandler(async (content) =>
        {
            await HandleAsync(aiService, content);
        }, contentArgument);
    }

    private static async Task HandleAsync(IAiService aiService, string content)
    {
        Console.WriteLine("🤖 Analyzing...\n");

        var explanation = await aiService.ExplainAsync(content);

        Console.WriteLine("📖 Explanation:");
        Console.WriteLine(explanation);
    }
}
