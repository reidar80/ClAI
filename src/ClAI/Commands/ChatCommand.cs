using System.CommandLine;
using ClAI.Models;
using ClAI.Services;

namespace ClAI.Commands;

/// <summary>
/// Interactive chat command for conversational AI assistance.
/// </summary>
public class ChatCommand : Command
{
    public ChatCommand(IAiService aiService) : base("chat", "Start an interactive chat session with the AI")
    {
        this.SetHandler(async () =>
        {
            await HandleAsync(aiService);
        });
    }

    private static async Task HandleAsync(IAiService aiService)
    {
        Console.WriteLine("🤖 ClAI Interactive Chat");
        Console.WriteLine("Type 'exit' or 'quit' to end the session.");
        Console.WriteLine("Type 'clear' to clear the screen.");
        Console.WriteLine(new string('─', 50));
        Console.WriteLine();

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("You: ");
            Console.ResetColor();

            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input))
            {
                continue;
            }

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("quit", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("👋 Goodbye!");
                break;
            }

            if (input.Equals("clear", StringComparison.OrdinalIgnoreCase))
            {
                Console.Clear();
                Console.WriteLine("🤖 ClAI Interactive Chat");
                Console.WriteLine("Type 'exit' or 'quit' to end the session.");
                Console.WriteLine(new string('─', 50));
                Console.WriteLine();
                continue;
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("AI: ");
            Console.ResetColor();

            var response = await aiService.GetCompletionAsync(new AiRequest
            {
                Prompt = input,
                Temperature = 0.7
            });

            if (response.Success)
            {
                Console.WriteLine(response.Content);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error: {response.Error}");
                Console.ResetColor();
            }

            Console.WriteLine();
        }
    }
}
