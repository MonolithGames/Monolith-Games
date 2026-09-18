# Development environment

Monolith supports a local interactive workflow and an optional persistent Azure
workflow. The repository contains source code, configuration, scripts, and
versioned documentation. Installed tools and generated data stay outside Git.

## Recommended arrangement

Use the local Windows 11 machine for interactive development when the machine
allows software installation and has adequate resources:

- VS Code and the repository
- .NET SDK and C/C++ toolchains
- Android Studio and Android SDK when Android work is needed
- Local device or emulator testing
- Browser access to the Monolith dashboard

Use Azure as the persistent build and service host when the local machine is
temporary, reset between library sessions, or unavailable:

- Persistent repository checkout or build workspace
- Long-running builds and background services
- SQLite/media backups when the application is hosted there
- Optional remote adapters and deployment commands

RDP is suitable for administration. For regular source editing, VS Code Remote
SSH is preferred when available because it keeps the editor responsive while
commands and files remain on the remote machine.

## Windows setup

1. Install Git, VS Code, and the .NET 10 SDK if permitted.
2. Clone the repository from its private remote.
3. Copy `monolith.env.example` to a local environment file and set a private
   development password.
4. Run the application with `dotnet run --project web/Monolith.Web/Monolith.Web.csproj`,
   or start the reproducible container with `./scripts/local-up.sh` from a
   supported shell.
5. Open `http://127.0.0.1:5187` and run `./scripts/local-check.sh`.
6. Install Android Studio, Unity, Unreal, Maya, or other large tools only when
   that workflow is needed. Record their versions in project documentation;
   do not commit their installers or SDK directories.

PowerShell users can run the .NET commands directly. The shell scripts require
an environment with Docker and a POSIX-compatible shell, such as WSL or Git
Bash.

## Linux or dev container setup

The checked-in dev container uses the .NET SDK and restores the web project
automatically. Open the repository in the container, then run:

```bash
cp .env.local.example .env.local
./scripts/local-up.sh
./scripts/local-check.sh
```

The local Docker volume `monolith-local-data` preserves database state and data
protection keys between restarts. Treat it as disposable development data and
back it up before moving important work to Azure.

## Azure role and cost controls

Azure is the persistent remote build and service host, not the default place for
interactive Android Studio or Steam use. Start with a general-purpose Windows
or Linux VM sized for the build workload. Add a GPU VM only if a specific
workflow requires GPU acceleration; Android emulators and games can have
virtualization, driver, latency, and anti-cheat limitations in cloud VMs.

To control cost:

- Stop or deallocate the VM when it is not needed.
- Keep only the persistent disks and required storage running.
- Use private networking and least-privilege credentials for deployments.
- Store secrets in the VM or a secret manager, never in Git.
- Prefer local builds until a remote build is reproducible.

## Files and data policy

Commit source code, configuration templates, tests, scripts, and documentation.
Do not commit:

- Android Studio, SDKs, emulator images, Unity, Unreal, Maya, or Steam installers
- Build outputs, downloaded dependencies, caches, VM disks, or local databases
- Passwords, API keys, Coinbase credentials, signing keys, or certificates

The checked-in `monolith.env.example` is the broad configuration reference.
Use `.env.local` for local values and keep it uncommitted.
