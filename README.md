# [Microsoft Build //localhost:dubai 2026](https://developer.microsoft.com/en-us/reactor/events/27261/)

![Microsoft Build 2026](img/banner-build-26.png)

Build //localhost is a global community-led initiative designed to bring the key takeaways, AI announcements, and technical content from Microsoft Build 2026 directly to local developer communities. Taking place throughout June 2026, these in-person events are aimed at developers and cloud engineers looking to build AI solutions on Azure.

## Custom Agent Harnesses with Microsoft Foundry and Agent Framework

This repository hosts the content and resources for the session *Custom Agent Harnesses with Microsoft Foundry and Agent Framework*, I presented in-person in Dubai, on June 9th, for **Build //localhost:dubai**.

*It is a customized session inspired on the session and materials from  [BRK243: Claw and Agent Harness in Microsoft Foundry](https://build.microsoft.com/en-US/sessions/BRK243), presented at Build 2026*. The original session materials are available in the [BRK243 repo](https://github.com/microsoft/Build26-BRK243-claw-and-agent-harness-in-microsoft-foundry).

### Session Description

Learn how to design and build custom agent harnesses that turn Microsoft Agent Framework from a single-agent SDK into an observable, controllable runtime for real applications. We will start with the theory behind harnesses: agent sessions, tools, context providers, planning and mode state, background agents, file access, code execution, workflow orchestration, human oversight, telemetry, and evaluation. Then we will move into a live .NET demo backed by Microsoft Foundry, progressively building a console harness that runs research, delegation, data processing, and sandboxed code execution scenarios. You will leave with practical patterns for composing Agent Framework agents, connecting them to Foundry project endpoints, and operating custom agent experiences with clear state, traceability, and control.

### 🏠 Getting started in your own environment

If you're following these steps at your own pace:

- Clone this repository
- Set up your development environment
- Configure Azure AI environment variables (see "Run the Agent Harness samples" below)

### 🧪 Run the Agent Harness samples

This repository includes runnable .NET samples under `src/Agent-Harness`.

1. Install prerequisites:
     - Windows 11 (Pro/Enterprise) with Hyper-V enabled (Hyperlight can use Hyper-V) or WSL with KVM-capable host (the Hyperlight sandbox runs code in micro-VMs). Check [docs/HYPERLIGHT_SETUP.md](./docs/HYPERLIGHT_SETUP.md) for details on setting up Hyperlight.
     - .NET 10 SDK
     - Azure CLI
     - An Azure AI Foundry project endpoint
     - A deployed LLM model in that project (the samples expect a deployment named `gpt-5.4`, but you can use any model - just update the environment variable accordingly)
     - Docker or Podman (for running the Aspire Dashboard, which is optional but recommended for observability)
2. Set environment variables:

    ```powershell
    $env:AZURE_AI_PROJECT_ENDPOINT = "https://<your-project>.services.ai.azure.com/api/projects/<your-project-name>"
    $env:AZURE_AI_MODEL_DEPLOYMENT_NAME = "gpt-5.4"
    ```

3. Build all sample projects from the repo root:

    ```powershell
    $projects = @(
        ".\\src\\Agent-Harness\\ConsoleReactiveFramework\\ConsoleReactiveFramework.csproj",
        ".\\src\\Agent-Harness\\ConsoleReactiveComponents\\ConsoleReactiveComponents.csproj",
        ".\\src\\Agent-Harness\\Harness_Shared_Console\\Harness_Shared_Console.csproj",
        ".\\src\\Agent-Harness\\Harness_Shared_Console_OpenAI\\Harness_Shared_Console_OpenAI.csproj",
        ".\\src\\Agent-Harness\\Harness_Step01_Research\\Harness_Step01_Research.csproj",
        ".\\src\\Agent-Harness\\Harness_Step02_Research_WithBackgroundAgents\\Harness_Step02_Research_WithBackgroundAgents.csproj",
        ".\\src\\Agent-Harness\\Harness_Step03_DataProcessing\\Harness_Step03_DataProcessing.csproj",
        ".\\src\\Agent-Harness\\Harness_Step04_CodeExecution\\Harness_Step04_CodeExecution.csproj"
    )

    foreach ($p in $projects) {
        dotnet build $p -nologo
    }
    ```

4. Run individual steps:

    ```powershell
    dotnet run --project .\src\Agent-Harness\Harness_Step01_Research\Harness_Step01_Research.csproj
    dotnet run --project .\src\Agent-Harness\Harness_Step02_Research_WithBackgroundAgents\Harness_Step02_Research_WithBackgroundAgents.csproj
    dotnet run --project .\src\Agent-Harness\Harness_Step03_DataProcessing\Harness_Step03_DataProcessing.csproj
    dotnet run --project .\src\Agent-Harness\Harness_Step04_CodeExecution\Harness_Step04_CodeExecution.csproj
    ```

For per-sample details and prompts to try, see `src/Agent-Harness/README.md`.

### 📚 Resources and Next Steps

- [Build 2026 next steps](https://aka.ms/build26-next-steps) - Explore lab and session repos to further your learning from Microsoft Build

### 🌟 Microsoft Learn MCP Server

The Microsoft Learn MCP Server gives your AI agent direct access to Microsoft's official documentation - grounded, up-to-date answers about the products and services covered in this session.

**VS Code** - One click installation:

[![Install in VS Code](https://img.shields.io/badge/VS_Code-Install_Microsoft_Learn_MCP-0098FF?style=flat-square&logo=visualstudiocode&logoColor=white)](https://vscode.dev/redirect/mcp/install?name=microsoft-learn&config=%7B%22type%22%3A%22http%22%2C%22url%22%3A%22https%3A%2F%2Flearn.microsoft.com%2Fapi%2Fmcp%22%7D)

**GitHub Copilot CLI** - Run this to install the Learn MCP Server as a plugin:

```text
/plugin install microsoftdocs/mcp
```

For more info, other clients, and to post questions, visit the [Learn MCP Server repo](https://aka.ms/learnmcp).

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft trademarks or logos is subject to and must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/legal/intellectualproperty/trademarks/usage/general). Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship. Any use of third-party trademarks or logos are subject to those third-party's policies.

## License

This project is licensed under the MIT License. See the [LICENSE](./LICENSE) file for details.  
Documentation in this repository is licensed under the [Creative Commons Attribution 4.0 License](https://creativecommons.org/licenses/by/4.0/). See the [LICENSE-DOCS](./LICENSE-DOCS) file for details.
