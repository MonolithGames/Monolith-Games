# System Overview

Monolith is a multi-technology repository. The active product is the .NET
Blazor application in `web/Monolith.Web`; native engine and game-tool folders
are staged integration boundaries. Monolith coordinates toolchains but does
not bundle or replace their compilers and editors.

Maya automation lives in `tools/maya/monolith_maya.py`. It provides a
deterministic phrase-to-scene entry point for procedural car and city geometry
and optional OBJ export. The Monolith driver can invoke a configured Maya
command after Maya and its Python environment are installed.
The Monolith build driver coordinates installed toolchains. It does not replace
GCC, Clang, MSVC, the .NET SDK, Unity, or Unreal compilers.

Supported driver targets are C/GCC, C++/G++, C/Clang, C++/Clang++, CMake/Ninja, .NET,
Maya, Unity, Unreal, and Visual Studio/MSBuild. A target is either built with
the detected host tool or invoked through its `MONOLITH_*_COMMAND` override.
