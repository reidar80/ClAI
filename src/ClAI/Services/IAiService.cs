using ClAI.Models;

namespace ClAI.Services;

/// <summary>
/// Interface for AI service operations.
/// </summary>
public interface IAiService
{
    /// <summary>
    /// Sends a request to the AI service and returns the response.
    /// </summary>
    Task<AiResponse> GetCompletionAsync(AiRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a command suggestion for a natural language task description.
    /// </summary>
    Task<CommandSuggestion> GetCommandSuggestionAsync(string taskDescription, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asks the AI to explain a command or error message.
    /// </summary>
    Task<string> ExplainAsync(string content, CancellationToken cancellationToken = default);
}
