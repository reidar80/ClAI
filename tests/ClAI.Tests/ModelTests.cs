using ClAI.Models;

namespace ClAI.Tests;

public class AiRequestTests
{
    [Fact]
    public void AiRequest_DefaultValues_AreSet()
    {
        var request = new AiRequest { Prompt = "Test prompt" };

        Assert.Equal("Test prompt", request.Prompt);
        Assert.Null(request.SystemContext);
        Assert.Equal(1000, request.MaxTokens);
        Assert.Equal(0.7, request.Temperature);
    }

    [Fact]
    public void AiRequest_CustomValues_AreSet()
    {
        var request = new AiRequest
        {
            Prompt = "Custom prompt",
            SystemContext = "Custom context",
            MaxTokens = 500,
            Temperature = 0.5
        };

        Assert.Equal("Custom prompt", request.Prompt);
        Assert.Equal("Custom context", request.SystemContext);
        Assert.Equal(500, request.MaxTokens);
        Assert.Equal(0.5, request.Temperature);
    }
}

public class AiResponseTests
{
    [Fact]
    public void AiResponse_SuccessfulResponse_HasCorrectProperties()
    {
        var response = new AiResponse
        {
            Content = "Response content",
            Success = true,
            TokensUsed = 150
        };

        Assert.True(response.Success);
        Assert.Equal("Response content", response.Content);
        Assert.Null(response.Error);
        Assert.Equal(150, response.TokensUsed);
    }

    [Fact]
    public void AiResponse_FailedResponse_HasError()
    {
        var response = new AiResponse
        {
            Content = "",
            Success = false,
            Error = "API error occurred"
        };

        Assert.False(response.Success);
        Assert.Empty(response.Content);
        Assert.Equal("API error occurred", response.Error);
    }
}

public class CommandSuggestionTests
{
    [Fact]
    public void CommandSuggestion_SafeCommand_NotDangerous()
    {
        var suggestion = new CommandSuggestion
        {
            Command = "Get-ChildItem",
            Explanation = "Lists files in directory",
            IsDangerous = false
        };

        Assert.False(suggestion.IsDangerous);
        Assert.Null(suggestion.Warning);
    }

    [Fact]
    public void CommandSuggestion_DangerousCommand_HasWarning()
    {
        var suggestion = new CommandSuggestion
        {
            Command = "Remove-Item -Recurse",
            Explanation = "Deletes files recursively",
            IsDangerous = true,
            Warning = "This will permanently delete files!"
        };

        Assert.True(suggestion.IsDangerous);
        Assert.NotNull(suggestion.Warning);
    }
}
