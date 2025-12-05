using System.CommandLine;
using System.Text.Json;
using ClAI.Configuration;

namespace ClAI.Commands;

/// <summary>
/// Command to configure ClAI settings.
/// </summary>
public class ConfigCommand : Command
{
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".clai",
        "config.json"
    );

    public ConfigCommand() : base("config", "Configure ClAI settings")
    {
        var setCommand = new Command("set", "Set a configuration value");
        var getCommand = new Command("get", "Get a configuration value");
        var showCommand = new Command("show", "Show all configuration values");
        var initCommand = new Command("init", "Initialize configuration with interactive prompts");

        // Set command
        var keyArgument = new Argument<string>("key", "Configuration key (provider, apiKey, model, endpoint)");
        var valueArgument = new Argument<string>("value", "Configuration value");
        setCommand.AddArgument(keyArgument);
        setCommand.AddArgument(valueArgument);
        setCommand.SetHandler(HandleSetAsync, keyArgument, valueArgument);

        // Get command
        var getKeyArgument = new Argument<string>("key", "Configuration key to retrieve");
        getCommand.AddArgument(getKeyArgument);
        getCommand.SetHandler(HandleGetAsync, getKeyArgument);

        // Show command
        showCommand.SetHandler(HandleShowAsync);

        // Init command
        initCommand.SetHandler(HandleInitAsync);

        AddCommand(setCommand);
        AddCommand(getCommand);
        AddCommand(showCommand);
        AddCommand(initCommand);
    }

    private static async Task HandleSetAsync(string key, string value)
    {
        var settings = await LoadSettingsAsync();

        switch (key.ToLower())
        {
            case "provider":
                settings.Provider = value;
                break;
            case "apikey":
                settings.ApiKey = value;
                break;
            case "model":
                settings.Model = value;
                break;
            case "endpoint":
                settings.Endpoint = value;
                break;
            case "maxtokens":
                if (int.TryParse(value, out var maxTokens))
                    settings.DefaultMaxTokens = maxTokens;
                else
                    Console.WriteLine("❌ Invalid value for maxtokens. Must be an integer.");
                return;
            case "temperature":
                if (double.TryParse(value, out var temp))
                    settings.DefaultTemperature = temp;
                else
                    Console.WriteLine("❌ Invalid value for temperature. Must be a number.");
                return;
            default:
                Console.WriteLine($"❌ Unknown configuration key: {key}");
                Console.WriteLine("Valid keys: provider, apiKey, model, endpoint, maxTokens, temperature");
                return;
        }

        await SaveSettingsAsync(settings);
        Console.WriteLine($"✅ Configuration updated: {key} = {MaskSensitiveValue(key, value)}");
    }

    private static async Task HandleGetAsync(string key)
    {
        var settings = await LoadSettingsAsync();

        var value = key.ToLower() switch
        {
            "provider" => settings.Provider,
            "apikey" => MaskSensitiveValue("apikey", settings.ApiKey ?? ""),
            "model" => settings.Model,
            "endpoint" => settings.Endpoint ?? "",
            "maxtokens" => settings.DefaultMaxTokens.ToString(),
            "temperature" => settings.DefaultTemperature.ToString(),
            _ => null
        };

        if (value == null)
        {
            Console.WriteLine($"❌ Unknown configuration key: {key}");
            Console.WriteLine("Valid keys: provider, apiKey, model, endpoint, maxTokens, temperature");
        }
        else
        {
            Console.WriteLine($"{key}: {value}");
        }
    }

    private static async Task HandleShowAsync()
    {
        var settings = await LoadSettingsAsync();

        Console.WriteLine("📋 ClAI Configuration:");
        Console.WriteLine($"  Provider:    {settings.Provider}");
        Console.WriteLine($"  API Key:     {MaskSensitiveValue("apikey", settings.ApiKey ?? "(not set)")}");
        Console.WriteLine($"  Model:       {settings.Model}");
        Console.WriteLine($"  Endpoint:    {settings.Endpoint ?? "(default)"}");
        Console.WriteLine($"  Max Tokens:  {settings.DefaultMaxTokens}");
        Console.WriteLine($"  Temperature: {settings.DefaultTemperature}");
        Console.WriteLine();
        Console.WriteLine($"📁 Config file: {ConfigPath}");
    }

    private static async Task HandleInitAsync()
    {
        Console.WriteLine("🔧 ClAI Configuration Setup\n");

        Console.WriteLine("Select AI Provider:");
        Console.WriteLine("  1. OpenAI");
        Console.WriteLine("  2. Azure OpenAI");
        Console.WriteLine("  3. Ollama (local)");
        Console.Write("Enter choice (1-3): ");

        var providerChoice = Console.ReadLine()?.Trim();
        var provider = providerChoice switch
        {
            "1" => "OpenAI",
            "2" => "AzureOpenAI",
            "3" => "Ollama",
            _ => "OpenAI"
        };

        string? apiKey = null;
        string? endpoint = null;
        string model = "gpt-4";

        if (provider != "Ollama")
        {
            Console.Write("Enter API Key: ");
            apiKey = Console.ReadLine()?.Trim();
        }

        if (provider == "AzureOpenAI")
        {
            Console.Write("Enter Azure OpenAI Endpoint: ");
            endpoint = Console.ReadLine()?.Trim();
        }
        else if (provider == "Ollama")
        {
            Console.Write("Enter Ollama endpoint (default: http://localhost:11434/v1/): ");
            endpoint = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(endpoint))
                endpoint = "http://localhost:11434/v1/";
            model = "llama2";
        }

        Console.Write($"Enter model name (default: {model}): ");
        var modelInput = Console.ReadLine()?.Trim();
        if (!string.IsNullOrEmpty(modelInput))
            model = modelInput;

        var settings = new AiSettings
        {
            Provider = provider,
            ApiKey = apiKey,
            Endpoint = endpoint,
            Model = model
        };

        await SaveSettingsAsync(settings);
        Console.WriteLine("\n✅ Configuration saved successfully!");
        await HandleShowAsync();
    }

    private static string MaskSensitiveValue(string key, string value)
    {
        if (key.ToLower() == "apikey" && !string.IsNullOrEmpty(value) && value.Length > 8)
        {
            return value[..4] + new string('*', value.Length - 8) + value[^4..];
        }
        return value;
    }

    public static async Task<AiSettings> LoadSettingsAsync()
    {
        if (!File.Exists(ConfigPath))
        {
            return new AiSettings();
        }

        try
        {
            var json = await File.ReadAllTextAsync(ConfigPath);
            return JsonSerializer.Deserialize<AiSettings>(json) ?? new AiSettings();
        }
        catch
        {
            return new AiSettings();
        }
    }

    private static async Task SaveSettingsAsync(AiSettings settings)
    {
        var directory = Path.GetDirectoryName(ConfigPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ConfigPath, json);
    }
}
