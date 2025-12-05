namespace ClAI.Models;

/// <summary>
/// Represents a request to the AI service.
/// </summary>
public class AiRequest
{
    /// <summary>
    /// The user's prompt or query.
    /// </summary>
    public required string Prompt { get; set; }

    /// <summary>
    /// Optional system context to guide the AI's behavior.
    /// </summary>
    public string? SystemContext { get; set; }

    /// <summary>
    /// Maximum tokens to generate in the response.
    /// </summary>
    public int MaxTokens { get; set; } = 1000;

    /// <summary>
    /// Temperature for response randomness (0.0 - 2.0).
    /// </summary>
    public double Temperature { get; set; } = 0.7;
}
