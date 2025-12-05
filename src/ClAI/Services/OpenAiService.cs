using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClAI.Configuration;
using ClAI.Models;

namespace ClAI.Services;

/// <summary>
/// OpenAI-compatible AI service implementation.
/// Works with OpenAI, Azure OpenAI, and Ollama.
/// </summary>
public class OpenAiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly AiSettings _settings;

    public OpenAiService(HttpClient httpClient, AiSettings settings)
    {
        _httpClient = httpClient;
        _settings = settings;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        var baseUrl = _settings.Provider.ToUpperInvariant() switch
        {
            "OPENAI" => "https://api.openai.com/v1/",
            "AZUREOPENAI" => _settings.Endpoint ?? throw new InvalidOperationException("Endpoint is required for Azure OpenAI"),
            "OLLAMA" => _settings.Endpoint ?? "http://localhost:11434/v1/",
            _ => _settings.Endpoint ?? "https://api.openai.com/v1/"
        };

        _httpClient.BaseAddress = new Uri(baseUrl);

        if (!string.IsNullOrEmpty(_settings.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        }
    }

    public async Task<AiResponse> GetCompletionAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var chatRequest = new ChatCompletionRequest
            {
                Model = _settings.Model,
                Messages = new List<ChatMessage>
                {
                    new() { Role = "system", Content = request.SystemContext ?? GetDefaultSystemContext() },
                    new() { Role = "user", Content = request.Prompt }
                },
                MaxTokens = request.MaxTokens > 0 ? request.MaxTokens : _settings.DefaultMaxTokens,
                Temperature = request.Temperature
            };

            var response = await _httpClient.PostAsJsonAsync("chat/completions", chatRequest, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return new AiResponse
                {
                    Content = string.Empty,
                    Success = false,
                    Error = $"API request failed: {response.StatusCode} - {errorContent}"
                };
            }

            var result = await response.Content.ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken: cancellationToken);

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                return new AiResponse
                {
                    Content = string.Empty,
                    Success = false,
                    Error = "No response received from AI service"
                };
            }

            return new AiResponse
            {
                Content = result.Choices[0].Message?.Content ?? string.Empty,
                Success = true,
                TokensUsed = result.Usage?.TotalTokens ?? 0
            };
        }
        catch (Exception ex)
        {
            return new AiResponse
            {
                Content = string.Empty,
                Success = false,
                Error = $"Error communicating with AI service: {ex.Message}"
            };
        }
    }

    public async Task<CommandSuggestion> GetCommandSuggestionAsync(string taskDescription, CancellationToken cancellationToken = default)
    {
        var systemContext = """
            You are a Windows command-line assistant. Given a task description, provide the appropriate PowerShell or CMD command to accomplish it.
            
            Respond in the following JSON format only:
            {
                "command": "the actual command to run",
                "explanation": "brief explanation of what the command does",
                "isDangerous": true/false,
                "warning": "warning message if dangerous, null otherwise"
            }
            
            Consider a command dangerous if it:
            - Deletes files or directories
            - Modifies system settings
            - Changes registry values
            - Could cause data loss
            - Requires administrator privileges
            
            Always suggest safe, reversible commands when possible.
            """;

        var response = await GetCompletionAsync(new AiRequest
        {
            Prompt = taskDescription,
            SystemContext = systemContext,
            Temperature = 0.3
        }, cancellationToken);

        if (!response.Success)
        {
            return new CommandSuggestion
            {
                Command = string.Empty,
                Explanation = response.Error ?? "Failed to get command suggestion",
                IsDangerous = false
            };
        }

        try
        {
            var suggestion = JsonSerializer.Deserialize<CommandSuggestion>(response.Content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return suggestion ?? new CommandSuggestion
            {
                Command = string.Empty,
                Explanation = "Failed to parse AI response",
                IsDangerous = false
            };
        }
        catch (JsonException)
        {
            // If JSON parsing fails, treat the response as plain text
            return new CommandSuggestion
            {
                Command = response.Content,
                Explanation = "AI provided a direct response",
                IsDangerous = true,
                Warning = "Could not verify command safety. Review before executing."
            };
        }
    }

    public async Task<string> ExplainAsync(string content, CancellationToken cancellationToken = default)
    {
        var systemContext = """
            You are a helpful Windows command-line assistant. Explain the following command or error message in simple terms.
            Be concise but thorough. If it's an error, suggest possible solutions.
            """;

        var response = await GetCompletionAsync(new AiRequest
        {
            Prompt = $"Explain this: {content}",
            SystemContext = systemContext,
            Temperature = 0.5
        }, cancellationToken);

        return response.Success ? response.Content : response.Error ?? "Failed to get explanation";
    }

    private static string GetDefaultSystemContext()
    {
        return """
            You are ClAI, a helpful Windows command-line AI assistant. 
            Help users with file management, searching, editing files, and managing computer settings.
            Provide clear, concise responses and command suggestions when appropriate.
            Always prioritize safety and warn about potentially dangerous operations.
            """;
    }

    #region API Models

    private class ChatCompletionRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4";

        [JsonPropertyName("messages")]
        public List<ChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; } = 1000;

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;
    }

    private class ChatMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    private class ChatCompletionResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("choices")]
        public List<Choice>? Choices { get; set; }

        [JsonPropertyName("usage")]
        public Usage? Usage { get; set; }
    }

    private class Choice
    {
        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("message")]
        public ChatMessage? Message { get; set; }

        [JsonPropertyName("finish_reason")]
        public string? FinishReason { get; set; }
    }

    private class Usage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("completion_tokens")]
        public int CompletionTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }

    #endregion
}
