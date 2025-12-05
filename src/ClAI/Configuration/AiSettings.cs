namespace ClAI.Configuration;

/// <summary>
/// Configuration settings for the AI service.
/// </summary>
public class AiSettings
{
    /// <summary>
    /// The AI provider to use (OpenAI, AzureOpenAI, Ollama).
    /// </summary>
    public string Provider { get; set; } = "OpenAI";

    /// <summary>
    /// API key for the AI service.
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Base URL for the AI service (required for Azure OpenAI and Ollama).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// Model name to use (e.g., gpt-4, gpt-3.5-turbo).
    /// </summary>
    public string Model { get; set; } = "gpt-4";

    /// <summary>
    /// Default maximum tokens for responses.
    /// </summary>
    public int DefaultMaxTokens { get; set; } = 1000;

    /// <summary>
    /// Default temperature for responses.
    /// </summary>
    public double DefaultTemperature { get; set; } = 0.7;
}
