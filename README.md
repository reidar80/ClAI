# ClAI PowerShell Module

ClAI (pronounced “clay”) is a local-first Windows PowerShell module that gives operators a natural-language interface to execute common DevOps and CLI tasks using local LLM runtimes and Model Context Protocol (MCP) tools. The project is designed to be auditable, hardware-aware, and extensible so new backends and skills can be added without changing the operator experience.

## High-level architecture

```text
ClAI/
  README.md                 # Overview and architecture notes
  src/
    ClAI.psd1               # Module manifest
    ClAI.psm1               # Module loader for public/private scripts
    Public/                 # Public cmdlets the operator calls
      Install-ClAI.ps1      # Installer entrypoint (hardware-aware)
      Invoke-ClAITask.ps1   # Main natural-language task runner
      Get-ClAIConfig.ps1    # Read configuration
      Set-ClAIConfig.ps1    # Update configuration
    Private/                # Internal helpers and abstractions
      HardwareDetection.ps1 # RAM/CPU/GPU/NPU detection
      ModelSelection.ps1    # Heuristics for Phi-3 Mini vs Llama 3.1
      Config.ps1            # JSON config and logging utilities
      PromptBuilder.ps1     # System/user prompt assembly
      LlmClient.ps1         # Backend abstraction + local HTTP impl (Ollama-style)
      ExecutionEngine.ps1   # Command preview, confirmation, execution
      MCPClient.ps1         # MCP registry, npm/node detection, process mgmt
      Skills.ps1            # Skill registry for future extensions
  tools/
    Install-ClAI.ps1        # Convenience wrapper for the installer
  examples/
    Example-Usage.ps1       # Demonstrates invoking ClAI and MCP flows
```

### Components
- **Installer:** Verifies prerequisites, inspects hardware, recommends a model (Phi-3 Mini for constrained machines, Llama 3.1 8B for stronger hardware), optionally pulls the model via Ollama, and writes user-scoped config.
- **Configuration:** JSON stored under the user profile (e.g., `%USERPROFILE%\AppData\Local\ClAI\config.json`). Tracks backend type, endpoint, selected model, safety level, and MCP registry path.
- **LLM Abstraction:** Local HTTP backend implementation compatible with Ollama. Future cloud backends can implement the same interface without changing public cmdlets.
- **Prompt Builder:** Crafts system/user prompts that separate plan vs. commands, emphasize safety, and inform the LLM about available tools/skills.
- **Execution Engine:** Presents generated commands, requests confirmation, executes safely, and logs results for auditing.
- **MCP Integration:** Maintains a registry of MCP servers, detects Node/NPM, prompts before installing/starting servers, and manages child processes.
- **Skills:** Lightweight registry to enrich prompts with domain context (Azure, Git, Kubernetes, etc.) and declare required tools.

### Operational flow
1. **Install:** `Install-ClAI` checks PowerShell/OS versions, detects hardware, recommends a model, and configures the local LLM backend (default Ollama). It copies the module into the user module path and writes configuration.
2. **Invoke task:** `Invoke-ClAITask` builds a prompt from user input and skill context, sends it to the selected backend, parses the plan/commands, previews them, and optionally executes with operator confirmation.
3. **MCP tools:** Operators use MCP helpers to register servers, install via npm (with approval), and start/stop them. Future function-calling integration can route LLM-suggested MCP tool calls through these controls.

### Design principles
- **Local-first & hardware-aware:** Prefers local LLMs; selects models based on RAM/CPU/GPU availability.
- **Operator control:** Always preview commands and ask before installations or server startups; configurable safety policies.
- **Extensible:** Clear separations between prompts, LLM backend, skills, and MCP adapters to enable new capabilities without breaking interfaces.
- **Auditable:** Optional logging of prompts, commands, and execution results stored under the user profile.

## Quick start
1. Run the installer (PowerShell):
   ```powershell
   Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
   ./tools/Install-ClAI.ps1 -Verbose
   ```
2. Load the module and run a task:
   ```powershell
   Import-Module ClAI
   Invoke-ClAITask -Prompt "Summarize current git status and propose cleanup commands" -PreviewOnly
   ```
3. Manage configuration:
   ```powershell
   Get-ClAIConfig
   Set-ClAIConfig -DefaultModel "phi3-mini" -SafetyLevel Conservative
   ```
4. Work with MCP servers:
   ```powershell
   Add-ClAIMcpServer -Name "example" -Source "npm:@org/example-mcp" -StartCommand "npx @org/example-mcp" -Port 8080
   Start-ClAIMcpServer -Name "example" -Confirm
   ```

## Limitations and next steps
- NPU detection is stubbed for future hardware APIs; currently marked as unknown when not discoverable.
- MCP function-calling loop is not yet wired to LLM outputs; operators start required tools manually using provided helpers.
- Cloud LLM backends are scaffolded via configuration but require specific implementations to be added later.
