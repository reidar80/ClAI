# ClAI - AI-Powered Windows CLI Assistant

A Windows command-line extension that leverages generative AI to help operators with everyday tasks like file management, searching, editing, and managing computer settings.

## Features

- **🤖 AI-Powered Commands**: Get intelligent command suggestions using natural language
- **💬 Interactive Chat**: Have conversations with AI for assistance
- **📁 File Management**: AI-assisted file searching, organization, and analysis
- **⚙️ System Management**: Get help with system settings, troubleshooting, and process analysis
- **🔧 Command Explanation**: Understand commands and error messages

## Installation

### Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- An AI provider account (OpenAI, Azure OpenAI, or local Ollama)

### Install as .NET Global Tool

```bash
# Clone the repository
git clone https://github.com/reidar80/ClAI.git
cd ClAI

# Build and pack the tool
dotnet pack src/ClAI/ClAI.csproj -c Release

# Install globally
dotnet tool install --global --add-source ./src/ClAI/nupkg ClAI
```

### Build from Source

```bash
# Clone the repository
git clone https://github.com/reidar80/ClAI.git
cd ClAI

# Build
dotnet build

# Run tests
dotnet test

# Run the CLI
dotnet run --project src/ClAI/ClAI.csproj -- --help
```

## Configuration

Before using ClAI, configure your AI provider:

```bash
# Interactive configuration
clai config init

# Or set values individually
clai config set provider OpenAI
clai config set apiKey YOUR_API_KEY
clai config set model gpt-4

# View current configuration
clai config show
```

### Supported AI Providers

| Provider | Description |
|----------|-------------|
| **OpenAI** | Use OpenAI's GPT models (default) |
| **Azure OpenAI** | Use Azure-hosted OpenAI models |
| **Ollama** | Use local AI models via Ollama |

### Configuration Options

| Key | Description | Default |
|-----|-------------|---------|
| `provider` | AI provider (OpenAI, AzureOpenAI, Ollama) | OpenAI |
| `apiKey` | API key for the AI service | - |
| `endpoint` | Custom endpoint URL (required for Azure, optional for Ollama) | - |
| `model` | AI model to use | gpt-4 |
| `maxTokens` | Maximum tokens per response | 1000 |
| `temperature` | Response randomness (0.0-2.0) | 0.7 |

## Usage

### Ask for Help with Tasks

```bash
# Get a command suggestion
clai ask "find all PDF files larger than 10MB"

# Get and execute the suggested command
clai ask "list running processes using most memory" --execute
```

### Explain Commands or Errors

```bash
# Explain a command
clai explain "Get-Process | Sort-Object CPU -Descending | Select-Object -First 10"

# Explain an error message
clai explain "The term 'git' is not recognized as the name of a cmdlet"
```

### Interactive Chat

```bash
# Start a chat session
clai chat
```

### File Management

```bash
# Search for files using natural language
clai file search "large log files from last week"

# Get organization suggestions for a directory
clai file organize ./Downloads

# Analyze a file or directory
clai file analyze ./my-project
```

### System Management

```bash
# Get system information and AI insights
clai system info

# Get help with a system setting
clai system setting "change default browser"

# Troubleshoot an issue
clai system troubleshoot "computer is running slow"

# Analyze running processes
clai system process
```

## Commands Reference

| Command | Description |
|---------|-------------|
| `clai ask <query>` | Get AI help with a task |
| `clai explain <content>` | Explain a command or error |
| `clai chat` | Start interactive chat |
| `clai file search <query>` | AI-powered file search |
| `clai file organize [path]` | Get file organization suggestions |
| `clai file analyze <path>` | Analyze file or directory |
| `clai system info` | Get system information |
| `clai system setting <query>` | Get help with settings |
| `clai system troubleshoot <issue>` | Get troubleshooting help |
| `clai system process` | Analyze running processes |
| `clai config init` | Interactive configuration |
| `clai config set <key> <value>` | Set configuration value |
| `clai config get <key>` | Get configuration value |
| `clai config show` | Show all configuration |

## Safety Features

ClAI prioritizes safety:

- ⚠️ **Dangerous Command Warnings**: Commands that could cause data loss or modify system settings are flagged
- ✅ **Confirmation Required**: The `--execute` flag requires explicit confirmation before running commands
- 🔒 **No Automatic Execution**: By default, commands are only suggested, not executed

## Project Structure

```
ClAI/
├── src/
│   └── ClAI/
│       ├── Commands/           # CLI command handlers
│       ├── Configuration/      # Settings and configuration
│       ├── Models/             # Data models
│       ├── Services/           # AI service integration
│       └── Program.cs          # Entry point
├── tests/
│   └── ClAI.Tests/            # Unit tests
├── ClAI.sln                   # Solution file
└── README.md
```

## Development

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Creating a Release

```bash
# Build release
dotnet publish src/ClAI/ClAI.csproj -c Release -r win-x64 --self-contained false

# Pack as tool
dotnet pack src/ClAI/ClAI.csproj -c Release
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- CLI parsing powered by [System.CommandLine](https://github.com/dotnet/command-line-api)
- Compatible with OpenAI, Azure OpenAI, and Ollama
