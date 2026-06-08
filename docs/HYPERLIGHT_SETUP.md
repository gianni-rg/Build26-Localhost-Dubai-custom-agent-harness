# Initial Hyperlight Setup

This guide captures the first-pass setup for trying Hyperlight and Hyperlight Sandbox from a Windows development machine.

Status as of 2026-06-05: the upstream Hyperlight project supports native Windows through Windows Hypervisor Platform. The `hyperlight-sandbox` SDK documentation currently lists Linux as the supported environment, with Windows support coming through Hyperlight. If your development machine does not meet the Windows requirements, use WSL2 Ubuntu for the Hyperlight Sandbox path.

## References

- [hyperlight-dev/hyperlight](https://github.com/hyperlight-dev/hyperlight)
- [hyperlight-dev/hyperlight-sandbox](https://github.com/hyperlight-dev/hyperlight-sandbox)
- [Hyperlight getting started](https://github.com/hyperlight-dev/hyperlight/blob/main/docs/getting-started.md)

## Windows Hypervisor Platform Requirements

Hyperlight requires hardware virtualization on the host. On Windows, this is provided by the **Windows Hypervisor Platform (WHP)** feature.

### OS and Hardware Requirements

| Requirement | Details |
| --- | --- |
| **OS Edition** | Windows 10/11 **Pro** or **Enterprise** (Home is not supported) |
| **Architecture** | AMD64 or Arm64 |
| **CPU** | 64-bit processor with **SLAT** (Second Level Address Translation) |
| **Virtualization** | Intel VT or AMD-V, enabled in BIOS/UEFI |
| **RAM** | Minimum 4 GB (8 GB recommended) |
| **DEP** | Hardware-enforced (Intel XD bit / AMD NX bit) |

### Verify Your System

Run from PowerShell to check if your hardware meets the requirements:

```powershell
systeminfo | Select-String -Pattern "Hyper-V|Virtualization"
```

All Hyper-V requirements should report **Yes**. If any item shows **No**, virtualization is likely disabled in BIOS/UEFI. Enable it before proceeding.

If the command returns this instead, it means the Windows hypervisor is already running and `systeminfo` is hiding the detailed requirement table:

```text
Hyper-V Requirements:          A hypervisor has been detected. Features required for Hyper-V will not be displayed.
```

For scripts, treat an already-running hypervisor as success. Some CIM CPU capability fields can also be unavailable or inaccurate after the hypervisor is active, so use them as hints rather than hard blockers.

### Enable Windows Hypervisor Platform (Automated)

Run the following from an **elevated PowerShell** prompt.

```powershell
# Enable WHP and Virtual Machine Platform (both are needed for coexistence with WSL2/Podman)
dism /Online /Enable-Feature /FeatureName:HypervisorPlatform /All /NoRestart
dism /Online /Enable-Feature /FeatureName:VirtualMachinePlatform /All /NoRestart

# Verify the features are enabled
dism /Online /Get-FeatureInfo /FeatureName:HypervisorPlatform
dism /Online /Get-FeatureInfo /FeatureName:VirtualMachinePlatform
```

A reboot is required:

```powershell
Restart-Computer -Force
```

### Full Automated Setup Script

Save this as `setup-hyperlight-prereqs.ps1` and run as Administrator:

```powershell
#Requires -RunAsAdministrator

Write-Host "=== Hyperlight Prerequisites Setup ===" -ForegroundColor Cyan

# 1. Check virtualization support. systeminfo and some CIM CPU fields can hide or misreport
#    details when a hypervisor is already running.
$computer = Get-CimInstance -ClassName Win32_ComputerSystem
$processors = Get-CimInstance -ClassName Win32_Processor

if ($computer.HypervisorPresent) {
    Write-Host "[OK] A Windows hypervisor is already running." -ForegroundColor Green
} else {
    $virtualizationEnabled = $processors | Where-Object { $_.VirtualizationFirmwareEnabled }
    $slatSupported = $processors | Where-Object { $_.SecondLevelAddressTranslationExtensions }

    if (-not $virtualizationEnabled) {
        Write-Error "Virtualization not enabled in BIOS/UEFI. Please enable Intel VT/AMD-V and reboot."
        exit 1
    }

    if (-not $slatSupported) {
        Write-Warning "CPU does not report SLAT support. Continuing because this value can be unavailable or inaccurate on some systems."
    }

    Write-Host "[OK] Virtualization is enabled in BIOS." -ForegroundColor Green
}

# 2. Enable required Windows features using DISM (more reliable than Enable-WindowsOptionalFeature)
$features = @("HypervisorPlatform", "VirtualMachinePlatform")
foreach ($feat in $features) {
    $info = dism /Online /Get-FeatureInfo /FeatureName:$feat 2>&1
    if ($info -match "State\s+:\s+Disabled") {
        Write-Host "Enabling $feat..." -ForegroundColor Yellow
        dism /Online /Enable-Feature /FeatureName:$feat /All /NoRestart
    } else {
        Write-Host "[OK] $feat is already enabled." -ForegroundColor Green
    }
}

# 3. Ensure WSL2 is installed
if (-not (wsl --status 2>$null)) {
    Write-Host "Installing WSL2..." -ForegroundColor Yellow
    wsl --install -d Ubuntu
} else {
    Write-Host "[OK] WSL2 is installed." -ForegroundColor Green
}

Write-Host ""
Write-Host "=== Setup complete. A reboot is required. ===" -ForegroundColor Cyan
Write-Host "Run: Restart-Computer -Force" -ForegroundColor Yellow
```

### Coexistence Notes

WHP, WSL2, Podman, and Hyperlight all share the same underlying hypervisor but run in separate VM namespaces, with **no conflicts**:

| Technology | Hypervisor Access | Conflict? |
| --- | --- | --- |
| WSL2 | Hyper-V VM layer | No |
| Windows Hypervisor Platform | User-mode API to hypervisor | No |
| Podman (via WSL2 backend) | Runs inside WSL2 VM | No |
| Hyperlight | Uses WHP for micro-VMs | No |

### Troubleshooting

- **"Hypervisor not found" error** - Verify WHP is enabled and you've rebooted:

  ```powershell
  bcdedit /enum | Select-String "hypervisorlaunchtype"
  ```

  Expected output: `hypervisorlaunchtype Auto`. If it shows `Started`, run:

  ```powershell
  bcdedit /set hypervisorlaunchtype auto
  Restart-Computer -Force
  ```

- **Windows Home edition** - WHP is not available on Windows Home. You would need Pro or Enterprise.

- **Third-party hypervisors** (VMware Workstation, VirtualBox) - These conflict with the Hyper-V hypervisor. Either uninstall them or use their Hyper-V mode (VMware Workstation 15+, VirtualBox 6.0+).

## Recommended Path: WSL2 Ubuntu

Use this path when you want to build or run `hyperlight-sandbox`, including Python, .NET, Rust, and example sandbox flows.

### 1. Install WSL2

Run from an elevated PowerShell prompt:

```powershell
wsl --install -d Ubuntu
wsl --update
```

Reboot if Windows asks, then open the Ubuntu terminal and finish the distro first-run setup.

### 2. Install Linux Build Prerequisites

Run inside Ubuntu/WSL:

```bash
sudo apt update
sudo apt install -y \
  build-essential pkg-config libssl-dev curl git clang llvm lld nodejs npm
```

Install the .NET SDKs. The upstream `hyperlight-sandbox` .NET SDK currently builds examples for `net8.0`, so .NET 8 is required at runtime. This setup also installs .NET 10 side-by-side.

Use the official `dotnet-install.sh` script so this works on Ubuntu releases where the Microsoft apt feed does not publish `dotnet-sdk-8.0`, such as Ubuntu 26.04:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh

./dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
./dotnet-install.sh --channel 10.0 --install-dir "$HOME/.dotnet"
rm dotnet-install.sh

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

grep -qxF 'export DOTNET_ROOT="$HOME/.dotnet"' "$HOME/.bashrc" || \
  echo 'export DOTNET_ROOT="$HOME/.dotnet"' >> "$HOME/.bashrc"
grep -qxF 'export PATH="$HOME/.dotnet:$PATH"' "$HOME/.bashrc" || \
  echo 'export PATH="$HOME/.dotnet:$PATH"' >> "$HOME/.bashrc"

dotnet --version
dotnet --list-sdks
dotnet --list-runtimes
```

Install Rust through `rustup`:

```bash
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
source "$HOME/.cargo/env"
rustup --version
cargo --version
```

Install the repo build helpers:

```bash
cargo install just
curl -LsSf https://astral.sh/uv/install.sh | sh
source "$HOME/.local/bin/env" 2>/dev/null || true
```

### 3. Verify Virtualization Access

Hyperlight needs a hardware virtualization backend. In WSL2, the expected backend is KVM.

```bash
ls -l /dev/kvm
```

If `/dev/kvm` exists but your user cannot access it, add yourself to the `kvm` group:

```bash
sudo usermod -aG kvm "$USER"
```

Close and reopen WSL.

If `/dev/kvm` is missing, check that virtualization is enabled in BIOS/UEFI and that WSL is up to date:

```powershell
wsl --update
wsl --status
```

### 4. Build Hyperlight Sandbox

Clone the upstream sandbox repo inside WSL, preferably outside this repo unless you are intentionally vendoring or comparing sources:

```bash
git clone https://github.com/hyperlight-dev/hyperlight-sandbox.git
cd hyperlight-sandbox
just build
just examples
```

Useful upstream commands:

```bash
just build
just test
just lint
just fmt
just examples
```

## Quick Python SDK Smoke Test

After the WSL prerequisites are installed, create a temporary Python sandbox environment:

```bash
uv venv
source .venv/bin/activate
uv pip install "hyperlight-sandbox[wasm,python_guest]"
```

Create `smoke.py`:

```python
from hyperlight_sandbox import Sandbox


sandbox = Sandbox(backend="wasm", module="python_guest.path")
sandbox.register_tool("add", lambda a=0, b=0: a + b)

result = sandbox.run(
    """
total = call_tool('add', a=3, b=4)
print(f"3 + 4 = {total}")
"""
)

print(result.stdout)
```

Run it:

```bash
python smoke.py
```

## Native Windows Path: Core Hyperlight Only

Use this path when experimenting with the core `hyperlight` repo directly.  
It is not the recommended path for `hyperlight-sandbox` today.

Requirements from the upstream Hyperlight docs:

- Windows 11 Pro, Enterprise, or Education, or Windows Server 2025 or later
- Windows Hypervisor Platform
- Rust through `rustup`
- Visual Studio Build Tools with the C++ workload and Windows SDK
- LLVM/Clang if building guest binaries

Enable Windows Hypervisor Platform from an elevated PowerShell prompt:

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName HypervisorPlatform -NoRestart
```

Reboot, then install the Hyperlight cargo helper:

```powershell
cargo install --locked cargo-hyperlight
```

Create and run a native Hyperlight sample:

```powershell
cargo hyperlight new my-project
cd my-project\guest
cargo hyperlight build
cd ..\host
cargo run
```

## Sandbox Troubleshooting

### No Hypervisor Found

If `just examples` fails with `No Hypervisor was found for Sandbox`, the build completed but Hyperlight could not open a virtualization backend from inside WSL. For WSL2, that backend should be `/dev/kvm`.

In WSL:

```bash
ls -l /dev/kvm
groups
```

If `/dev/kvm` exists, confirm your user is in the owning group. If not, add the group and reopen WSL:

```bash
sudo usermod -aG kvm "$USER"
```

If `/dev/kvm` is missing, confirm the distro is running as WSL2 from PowerShell:

```powershell
wsl --list --verbose
wsl --status
```

If the distro is WSL1, convert it:

```powershell
wsl --set-version Ubuntu 2
```

Then enable the Windows virtualization features from an elevated PowerShell prompt and reboot:

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName VirtualMachinePlatform -NoRestart
Enable-WindowsOptionalFeature -Online -FeatureName HypervisorPlatform -NoRestart
bcdedit /set hypervisorlaunchtype auto
```

If `/dev/kvm` is still missing after reboot, make sure WSL nested virtualization is not disabled. Create or update `%UserProfile%\.wslconfig`:

```ini
[wsl2]
nestedVirtualization=true
```

Restart WSL and check again:

```powershell
wsl --shutdown
wsl --update
wsl
```

Then inside WSL:

```bash
ls -l /dev/kvm
just examples
```

On native Windows, inspect optional features from an elevated PowerShell prompt:

```powershell
Get-WindowsOptionalFeature -Online |
  Where-Object {
    $_.FeatureName -match 'Hyper-V|HypervisorPlatform|VirtualMachinePlatform'
  } |
  Format-Table
```

### Build Tool Errors

- In WSL, confirm `build-essential`, `clang`, `llvm`, and `lld` are installed.
- In WSL, confirm .NET 8 and .NET 10 are installed with `dotnet --list-sdks`.
- On Windows, confirm Visual Studio Build Tools has the C++ workload and Windows SDK installed.
- Confirm Rust is available in the active shell with `cargo --version`.

### Missing .NET 8 Runtime

If a .NET example fails with this message:

```text
Framework: 'Microsoft.NETCore.App', version '8.0.0' (x64)
The following frameworks were found:
  10.0.x at [/usr/lib/dotnet/shared/Microsoft.NETCore.App]
```

install the .NET 8 SDK into the same `~/.dotnet` root as .NET 10:

```bash
curl -fsSL https://dot.net/v1/dotnet-install.sh -o dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
rm dotnet-install.sh

export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"

dotnet --list-runtimes
```

The `hyperlight-sandbox` .NET examples currently target `net8.0`. A newer major .NET runtime, such as .NET 10, does not automatically satisfy that target.
