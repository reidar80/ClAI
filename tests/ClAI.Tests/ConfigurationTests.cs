using ClAI.Configuration;

namespace ClAI.Tests;

public class AiSettingsTests
{
    [Fact]
    public void AiSettings_DefaultValues_AreCorrect()
    {
        var settings = new AiSettings();

        Assert.Equal("OpenAI", settings.Provider);
        Assert.Null(settings.ApiKey);
        Assert.Null(settings.Endpoint);
        Assert.Equal("gpt-4", settings.Model);
        Assert.Equal(1000, settings.DefaultMaxTokens);
        Assert.Equal(0.7, settings.DefaultTemperature);
    }

    [Fact]
    public void AiSettings_CustomValues_AreSet()
    {
        var settings = new AiSettings
        {
            Provider = "AzureOpenAI",
            ApiKey = "test-key",
            Endpoint = "https://my-endpoint.azure.com",
            Model = "gpt-35-turbo",
            DefaultMaxTokens = 2000,
            DefaultTemperature = 0.5
        };

        Assert.Equal("AzureOpenAI", settings.Provider);
        Assert.Equal("test-key", settings.ApiKey);
        Assert.Equal("https://my-endpoint.azure.com", settings.Endpoint);
        Assert.Equal("gpt-35-turbo", settings.Model);
        Assert.Equal(2000, settings.DefaultMaxTokens);
        Assert.Equal(0.5, settings.DefaultTemperature);
    }

    [Theory]
    [InlineData("OpenAI")]
    [InlineData("AzureOpenAI")]
    [InlineData("Ollama")]
    public void AiSettings_SupportsMultipleProviders(string provider)
    {
        var settings = new AiSettings { Provider = provider };
        Assert.Equal(provider, settings.Provider);
    }
}
