# Monolith compiler command

`monolith.py` is a unified build driver, not a replacement compiler. It
selects the installed compiler or editor for each target:

```text
python3 tools/monolith/monolith.py doctor
python3 tools/monolith/monolith.py build c
python3 tools/monolith/monolith.py build cpp
python3 tools/monolith/monolith.py build clang
python3 tools/monolith/monolith.py build cmake
python3 tools/monolith/monolith.py build web
python3 tools/monolith/monolith.py build maya
python3 tools/monolith/monolith.py build unity
python3 tools/monolith/monolith.py build unreal
python3 tools/monolith/monolith.py build visualstudio
python3 tools/monolith/monolith.py build all
```

A true language compiler for every supported ecosystem would require separate
frontends and backends. Monolith keeps those mature toolchains underneath one
consistent command instead. Clang and CMake/Ninja are supported native build
drivers. Maya, Unity, Unreal, and Visual Studio commands are supplied by
`MONOLITH_MAYA_COMMAND`, `MONOLITH_UNITY_BUILD_COMMAND`, and
`MONOLITH_UNREAL_COMMAND`; Visual Studio uses `MONOLITH_MSBUILD_COMMAND`.
