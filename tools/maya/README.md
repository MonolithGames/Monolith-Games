# Monolith Maya plugin

`monolith_maya.py` is a Maya Python starter plugin for phrase-driven procedural
scene generation. It recognizes car, city, urban, street, vehicle, and
automotive phrases, builds grouped primitive geometry, and can export the
selected result to OBJ.

Inside Maya:

```python
import sys
sys.path.append(r"/path/to/Monolith-Games/tools/maya")
import monolith_maya
monolith_maya.generate_from_phrase("a dense city with sports cars", r"C:/tmp/monolith_scene.obj")
```

This is deterministic procedural generation, not a general AI model generator.
More complex authored generators or an approved asset/AI service can be added
behind the same `generate_from_phrase` entry point.
