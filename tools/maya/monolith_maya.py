"""Monolith Maya procedural phrase generator.

Run inside Maya with:
    import monolith_maya
    monolith_maya.generate_from_phrase("a dense city with sports cars", "C:/tmp/scene.obj")
"""

from __future__ import annotations

import re
from typing import Optional

import maya.cmds as cmds


MODULE_NAME = "MonolithGenerated"


def _cube(name, position, scale, parent):
    node = cmds.polyCube(name=name)[0]
    cmds.xform(node, translation=position, scale=scale)
    cmds.parent(node, parent)
    return node


def _cylinder(name, position, radius, depth, rotation, parent):
    node = cmds.polyCylinder(name=name, radius=radius, height=depth, subdivisionsAxis=16)[0]
    cmds.xform(node, translation=position, rotation=rotation)
    cmds.parent(node, parent)
    return node


def _new_group(name):
    existing = cmds.ls(name, long=True)
    if existing:
        cmds.delete(existing)
    return cmds.group(empty=True, name=name)


def create_car(parent, index=0, position=(0, 0, 0)):
    """Create a stylized low-poly car from primitive geometry."""
    group = cmds.group(empty=True, name=f"MonolithCar_{index}", parent=parent)
    x, y, z = position
    _cube(f"CarBody_{index}", (x, y + 0.7, z), (2.4, 0.45, 1.05), group)
    _cube(f"CarCabin_{index}", (x + 0.2, y + 1.35, z), (1.35, 0.45, 0.85), group)
    for wheel_index, wheel_x in enumerate((-1.65, 1.65)):
        for wheel_z in (-1.05, 1.05):
            _cylinder(f"CarWheel_{index}_{wheel_index}_{wheel_z}", (x + wheel_x, y + 0.45, z + wheel_z), 0.48, 0.32, (90, 0, 0), group)
    _cube(f"CarLight_{index}_front", (x + 2.35, y + 0.85, z), (0.12, 0.2, 0.65), group)
    return group


def create_city(parent, width=7, depth=7):
    """Create a procedural city block with roads and varied buildings."""
    group = cmds.group(empty=True, name="MonolithCity", parent=parent)
    for x in range(-width, width + 1, 2):
        _cube(f"RoadX_{x}", (x * 3, 0, 0), (0.9, 0.05, depth * 3), group)
    for z in range(-depth, depth + 1, 2):
        _cube(f"RoadZ_{z}", (0, 0.02, z * 3), (width * 3, 0.05, 0.9), group)
    building_index = 0
    for x in range(-width, width + 1, 2):
        for z in range(-depth, depth + 1, 2):
            height = 2 + ((abs(x * 7 + z * 11) % 7))
            _cube(f"Building_{building_index}", (x * 3, height, z * 3), (1.1, height, 1.1), group)
            building_index += 1
    return group


def generate_from_phrase(phrase: str, export_path: Optional[str] = None):
    """Generate a scene from a phrase and optionally export selected groups to OBJ."""
    if not phrase or not phrase.strip():
        raise ValueError("A generation phrase is required")

    normalized = re.sub(r"[^a-z0-9 ]+", " ", phrase.lower())
    root = _new_group(MODULE_NAME)
    wants_city = "city" in normalized or "urban" in normalized or "street" in normalized
    wants_car = "car" in normalized or "vehicle" in normalized or "automotive" in normalized
    if not wants_city and not wants_car:
        wants_city = True
        wants_car = True

    if wants_city:
        create_city(root)
    if wants_car:
        create_car(root, position=(0, 1, 0))

    cmds.select(root, replace=True)
    if export_path:
        try:
            cmds.loadPlugin("objExport", quiet=True)
        except RuntimeError:
            pass
        cmds.file(export_path, force=True, type="OBJexport", exportSelected=True)
    return root


def initializePlugin(plugin):
    """Maya plugin entry point."""
    return None


def uninitializePlugin(plugin):
    """Maya plugin shutdown entry point."""
    return None
