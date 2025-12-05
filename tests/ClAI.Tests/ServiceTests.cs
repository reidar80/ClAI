using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;
using ClAI.Configuration;
using ClAI.Models;
using ClAI.Services;

namespace ClAI.Tests;

public class OpenAiServiceTests
{
    private static HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object responseContent)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(responseContent)
            });

        return new HttpClient(handlerMock.Object);
    }

    [Fact]
    public async Task GetCompletionAsync_Success_ReturnsContent()
    {
        var mockResponse = new
        {
            id = "test-id",
            choices = new[]
            {
                new
                {
                    index = 0,
                    message = new { role = "assistant", content = "Hello, I am an AI assistant." },
                    finish_reason = "stop"
                }
            },
            usage = new { prompt_tokens = 10, completion_tokens = 20, total_tokens = 30 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var settings = new AiSettings { ApiKey = "test-key" };
        var service = new OpenAiService(httpClient, settings);

        var result = await service.GetCompletionAsync(new AiRequest { Prompt = "Hello" });

        Assert.True(result.Success);
        Assert.Equal("Hello, I am an AI assistant.", result.Content);
        Assert.Equal(30, result.TokensUsed);
    }

    [Fact]
    public async Task GetCompletionAsync_ApiError_ReturnsFailure()
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Invalid API key")
            });

        var httpClient = new HttpClient(handlerMock.Object);
        var settings = new AiSettings { ApiKey = "invalid-key" };
        var service = new OpenAiService(httpClient, settings);

        var result = await service.GetCompletionAsync(new AiRequest { Prompt = "Hello" });

        Assert.False(result.Success);
        Assert.Contains("Unauthorized", result.Error);
    }

    [Fact]
    public async Task GetCompletionAsync_EmptyResponse_ReturnsFailure()
    {
        var mockResponse = new
        {
            id = "test-id",
            choices = Array.Empty<object>(),
            usage = new { total_tokens = 0 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var settings = new AiSettings { ApiKey = "test-key" };
        var service = new OpenAiService(httpClient, settings);

        var result = await service.GetCompletionAsync(new AiRequest { Prompt = "Hello" });

        Assert.False(result.Success);
        Assert.Equal("No response received from AI service", result.Error);
    }

    [Fact]
    public async Task GetCommandSuggestionAsync_ValidJson_ParsesCorrectly()
    {
        var commandJson = """
        {
            "command": "Get-ChildItem -Path C:\\ -Recurse",
            "explanation": "Lists all files recursively",
            "isDangerous": false,
            "warning": null
        }
        """;

        var mockResponse = new
        {
            id = "test-id",
            choices = new[]
            {
                new { index = 0, message = new { role = "assistant", content = commandJson }, finish_reason = "stop" }
            },
            usage = new { total_tokens = 50 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var settings = new AiSettings { ApiKey = "test-key" };
        var service = new OpenAiService(httpClient, settings);

        var result = await service.GetCommandSuggestionAsync("List all files");

        Assert.Equal("Get-ChildItem -Path C:\\ -Recurse", result.Command);
        Assert.Equal("Lists all files recursively", result.Explanation);
        Assert.False(result.IsDangerous);
    }

    [Fact]
    public async Task ExplainAsync_Success_ReturnsExplanation()
    {
        var mockResponse = new
        {
            id = "test-id",
            choices = new[]
            {
                new { index = 0, message = new { role = "assistant", content = "This command lists directory contents." }, finish_reason = "stop" }
            },
            usage = new { total_tokens = 25 }
        };

        var httpClient = CreateMockHttpClient(HttpStatusCode.OK, mockResponse);
        var settings = new AiSettings { ApiKey = "test-key" };
        var service = new OpenAiService(httpClient, settings);

        var result = await service.ExplainAsync("dir /s");

        Assert.Equal("This command lists directory contents.", result);
    }
}
