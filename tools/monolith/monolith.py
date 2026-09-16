#!/usr/bin/env python3
"""Unified Monolith build driver for installed project toolchains."""

from __future__ import annotations

import argparse
import os
import shlex
import shutil
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]


def tool(name: str) -> str:
    return shutil.which(name) or "not found"


def doctor() -> int:
    checks = {
        "gcc": tool("gcc"),
        "g++": tool("g++"),
        "clang": tool("clang"),
        "clang++": tool("clang++"),
        "cmake": tool("cmake"),
        "ninja": tool("ninja"),
        "dotnet": tool("dotnet"),
        "node": tool("node"),
        "npm": tool("npm"),
        "mayapy": tool("mayapy"),
        "maya": tool("maya"),
        "unity": tool("Unity"),
        "unreal": tool("UnrealEditor"),
        "msbuild": tool("msbuild"),
    }
    for name, location in checks.items():
        print(f"{name:>7}: {location}")
    return 0


def run(command: list[str]) -> None:
    print("+", " ".join(command))
    subprocess.run(command, cwd=ROOT, check=True)


def build_c() -> None:
    if tool("gcc") == "not found":
        raise RuntimeError("gcc is required for the C target")
    output = ROOT / "build" / "monolith-engine-c.o"
    output.parent.mkdir(exist_ok=True)
    run(["gcc", "-Iengine/c/include", "-Iengine/shared", "-c", "engine/c/src/main.c", "-o", str(output)])


def build_cpp() -> None:
    if tool("g++") == "not found":
        raise RuntimeError("g++ is required for the C++ target")
    output = ROOT / "build" / "monolith-engine-cpp.o"
    output.parent.mkdir(exist_ok=True)
    run(["g++", "-Iengine/cpp/include", "-Iengine/shared", "-c", "engine/cpp/src/main.cpp", "-o", str(output)])


def build_clang() -> None:
    if tool("clang") == "not found" or tool("clang++") == "not found":
        raise RuntimeError("clang and clang++ are required for the Clang target")
    output = ROOT / "build" / "monolith-engine-clang.o"
    output.parent.mkdir(exist_ok=True)
    run(["clang", "-Iengine/c/include", "-Iengine/shared", "-c", "engine/c/src/main.c", "-o", str(output)])
    output_cpp = ROOT / "build" / "monolith-engine-clang-cpp.o"
    run(["clang++", "-Iengine/cpp/include", "-Iengine/shared", "-c", "engine/cpp/src/main.cpp", "-o", str(output_cpp)])


def build_cmake() -> None:
    if tool("cmake") == "not found":
        raise RuntimeError("cmake is required for the CMake target")
    build_dir = ROOT / "build" / "cmake"
    configure = ["cmake", "-S", ".", "-B", str(build_dir)]
    if tool("ninja") != "not found":
        configure.extend(["-G", "Ninja"])
    run(configure)
    run(["cmake", "--build", str(build_dir)])


def build_web() -> None:
    if tool("dotnet") == "not found":
        raise RuntimeError("dotnet is required for the web target")
    run(["dotnet", "build", "web/Monolith.Web/Monolith.Web.csproj"])
    run(["dotnet", "test", "web/Monolith.Web.Tests/Monolith.Web.Tests.csproj"])


def build_external(target: str, environment_name: str, fallback_message: str) -> None:
    configured = os.environ.get(environment_name, "").strip()
    if configured:
        run(shlex.split(configured))
        return
    print(f"{target}: {fallback_message}")


def build_maya() -> None:
    build_external(
        "maya",
        "MONOLITH_MAYA_COMMAND",
        "plugin ready at tools/maya/monolith_maya.py; set MONOLITH_MAYA_COMMAND to invoke Maya",
    )


def build_unity() -> None:
    build_external(
        "unity",
        "MONOLITH_UNITY_BUILD_COMMAND",
        "project scaffold ready; set MONOLITH_UNITY_BUILD_COMMAND to invoke Unity batch mode",
    )


def build_unreal() -> None:
    build_external(
        "unreal",
        "MONOLITH_UNREAL_COMMAND",
        "project scaffold ready; set MONOLITH_UNREAL_COMMAND to invoke Unreal build tools",
    )


def build_visualstudio() -> None:
    build_external(
        "visualstudio",
        "MONOLITH_MSBUILD_COMMAND",
        "solution scaffold ready; install Visual Studio/MSBuild and set MONOLITH_MSBUILD_COMMAND",
    )


def build(target: str) -> int:
    actions = {"c": build_c, "cpp": build_cpp, "clang": build_clang, "cmake": build_cmake, "web": build_web, "maya": build_maya, "unity": build_unity, "unreal": build_unreal, "visualstudio": build_visualstudio}
    if target == "all":
        for name in ("c", "cpp", "cmake", "web", "maya", "unity", "unreal", "visualstudio"):
            actions[name]()
        return 0
    if target not in actions:
        raise ValueError(f"unknown target: {target}")
    actions[target]()
    return 0


parser = argparse.ArgumentParser(prog="monolith", description=__doc__)
subparsers = parser.add_subparsers(dest="command", required=True)
subparsers.add_parser("doctor")
build_parser = subparsers.add_parser("build")
build_parser.add_argument("target", choices=("c", "cpp", "clang", "cmake", "web", "maya", "unity", "unreal", "visualstudio", "all"))
args = parser.parse_args()

try:
    status = doctor() if args.command == "doctor" else build(args.target)
except (RuntimeError, subprocess.CalledProcessError, ValueError) as error:
    print(f"Monolith: {error}", file=sys.stderr)
    status = 1
sys.exit(status)
