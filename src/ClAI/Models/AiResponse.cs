namespace ClAI.Models;

/// <summary>
/// Represents a response from the AI service.
/// </summary>
public class AiResponse
{
    /// <summary>
    /// The generated text content.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Whether the request was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if the request failed.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Number of tokens used in the request.
    /// </summary>
    public int TokensUsed { get; set; }
}
