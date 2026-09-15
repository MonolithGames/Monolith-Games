# Monolith

Monolith is a Windows game-production client organized around template-driven
jobs. Version 1 provides the native dashboard, template selection, a visible
pipeline, a job log, and a JSON manifest for each started job.

## Project boundaries

- **Monolith Client**: `src/platform.c` and `src/main.c` provide the Win32 UI.
- **Pipeline Orchestrator**: `src/pipeline.c` advances the production stages and
	writes `build/monolith_job.log` and `build/monolith_job.json`.
- **Template Catalog**: the initial templates are Skyline Runner, Neon Kart,
	and Pocket Planet.
- **Azure Worker Adapter**: represented by the Azure pipeline stage; it will
	connect to a configured Windows VM in a later integration.
- **Maya Adapter**: represented by the Maya stage; it will run configured Maya
	scripts when Maya is installed on the worker.
- **Unity Adapter**: represented by the Unity stage; it will invoke a Unity
	batch build against the selected template.
- **Publishing Adapter**: represented by media and publishing-approval stages.
	Store credentials and a human release approval are required before adding
	real store API calls.

## Build releases

On the Windows build environment with MinGW and `windres` installed:

```text
make VERSION=1 release
make VERSION=2 release
```

The resulting files are `build/Monolith_v1.exe` and `build/Monolith_v2.exe`.
The embedded Windows product version follows the requested version number.

The current pipeline is deliberately adapter-aware: it records the complete
workflow and its outputs without claiming that Azure, Unity, Maya, payment,
advertising, multiplayer, or store credentials are configured. Those services
must be connected and tested on the target Windows worker before production
publishing is enabled.