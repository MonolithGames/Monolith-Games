// Simple 3D Match-3 prototype using Three.js and an OBJ gem model.
// Grid logic: 8x8, simple matching by color index. Click to swap adjacent gems.

(() => {
  const container = document.getElementById('game-container');
  const scene = new THREE.Scene();
  const camera = new THREE.PerspectiveCamera(45, window.innerWidth / window.innerHeight, 0.1, 1000);
  const renderer = new THREE.WebGLRenderer({ antialias: true });
  renderer.setSize(window.innerWidth, window.innerHeight);
  container.appendChild(renderer.domElement);

  const ambient = new THREE.AmbientLight(0xffffff, 0.8);
  scene.add(ambient);
  const dir = new THREE.DirectionalLight(0xffffff, 0.6);
  dir.position.set(5, 10, 7);
  scene.add(dir);

  camera.position.set(0, 12, 16);
  camera.lookAt(0, 0, 0);

  const GRID = { cols: 8, rows: 8, spacing: 1.6 };
  const colors = [0xff4b4b, 0x4bff8a, 0x4bb7ff, 0xffd24b, 0xcd4bff];

  let gemMeshPrototype = null;
  const tiles = [];

  const loader = new THREE.OBJLoader();
  // Load local gem.obj; fallback to a simple geometry if OBJ not found or fails.
  loader.load('gem.obj', (obj) => {
    gemMeshPrototype = obj.children[0];
    gemMeshPrototype.geometry.computeVertexNormals();
    gemMeshPrototype.material = new THREE.MeshStandardMaterial({ color: 0xffffff });
    initGrid();
  }, undefined, (err) => {
    console.warn('OBJ load failed, using box fallback', err);
    const geo = new THREE.OctahedronGeometry(0.6);
    const mat = new THREE.MeshStandardMaterial({ color: 0xffffff });
    gemMeshPrototype = new THREE.Mesh(geo, mat);
    initGrid();
  });

  function initGrid() {
    for (let r = 0; r < GRID.rows; r++) {
      tiles[r] = [];
      for (let c = 0; c < GRID.cols; c++) {
        const colorIndex = Math.floor(Math.random() * colors.length);
        const gem = createGem(colorIndex);
        const x = (c - (GRID.cols - 1) / 2) * GRID.spacing;
        const z = (r - (GRID.rows - 1) / 2) * GRID.spacing;
        gem.position.set(x, 0, z);
        scene.add(gem);
        tiles[r][c] = { mesh: gem, color: colorIndex, r, c };
      }
    }
    animate();
  }

  function createGem(colorIndex) {
    let mesh;
    if (gemMeshPrototype.isMesh) {
      mesh = gemMeshPrototype.clone();
      mesh.material = gemMeshPrototype.material.clone();
    } else {
      mesh = gemMeshPrototype.clone();
    }
    mesh.material.color.setHex(colors[colorIndex]);
    mesh.scale.setScalar(1.2);
    mesh.castShadow = true;
    return mesh;
  }

  // Simple picking / swap logic
  const raycaster = new THREE.Raycaster();
  const mouse = new THREE.Vector2();
  let firstPick = null;

  renderer.domElement.addEventListener('pointerdown', (e) => {
    mouse.x = (e.clientX / window.innerWidth) * 2 - 1;
    mouse.y = -(e.clientY / window.innerHeight) * 2 + 1;
    raycaster.setFromCamera(mouse, camera);
    const intersects = raycaster.intersectObjects(scene.children, true);
    if (intersects.length > 0) {
      const mesh = findTopLevelMesh(intersects[0].object);
      const tile = findTileByMesh(mesh);
      if (!tile) return;
      if (!firstPick) {
        firstPick = tile;
        highlight(tile.mesh, true);
      } else {
        // if adjacent, swap
        if (isAdjacent(firstPick, tile)) {
          swapTiles(firstPick, tile);
        }
        highlight(firstPick.mesh, false);
        firstPick = null;
      }
    }
  });

  function findTopLevelMesh(obj) {
    while (obj && !obj.parent.isScene) obj = obj.parent;
    return obj;
  }

  function findTileByMesh(mesh) {
    for (let r = 0; r < GRID.rows; r++) {
      for (let c = 0; c < GRID.cols; c++) {
        if (tiles[r][c].mesh === mesh) return tiles[r][c];
      }
    }
    return null;
  }

  function isAdjacent(a, b) {
    const dr = Math.abs(a.r - b.r);
    const dc = Math.abs(a.c - b.c);
    return (dr + dc) === 1;
  }

  function swapTiles(a, b) {
    // swap data
    const tempColor = a.color;
    a.color = b.color;
    b.color = tempColor;
    // update materials
    a.mesh.material.color.setHex(colors[a.color]);
    b.mesh.material.color.setHex(colors[b.color]);
    // check matches
    setTimeout(() => {
      const matches = findAllMatches();
      if (matches.length > 0) {
        removeMatches(matches);
      } else {
        // revert swap if no match
        const t = a.color; a.color = b.color; b.color = t;
        a.mesh.material.color.setHex(colors[a.color]);
        b.mesh.material.color.setHex(colors[b.color]);
      }
    }, 200);
  }

  function highlight(mesh, on) {
    if (on) mesh.scale.setScalar(1.4);
    else mesh.scale.setScalar(1.2);
  }

  function findAllMatches() {
    const matches = [];
    // rows
    for (let r = 0; r < GRID.rows; r++) {
      let run = [tiles[r][0]];
      for (let c = 1; c < GRID.cols; c++) {
        if (tiles[r][c].color === run[run.length - 1].color) run.push(tiles[r][c]);
        else { if (run.length >= 3) matches.push(run.slice()); run = [tiles[r][c]]; }
      }
      if (run.length >= 3) matches.push(run.slice());
    }
    // cols
    for (let c = 0; c < GRID.cols; c++) {
      let run = [tiles[0][c]];
      for (let r = 1; r < GRID.rows; r++) {
        if (tiles[r][c].color === run[run.length - 1].color) run.push(tiles[r][c]);
        else { if (run.length >= 3) matches.push(run.slice()); run = [tiles[r][c]]; }
      }
      if (run.length >= 3) matches.push(run.slice());
    }
    return matches;
  }

  function removeMatches(matches) {
    const removeSet = new Set();
    matches.forEach(run => run.forEach(t => removeSet.add(t)));
    // remove meshes and replace with new random gems dropping from top
    removeSet.forEach(t => {
      scene.remove(t.mesh);
    });
    // for each column, drop down colors
    for (let c = 0; c < GRID.cols; c++) {
      const col = [];
      for (let r = GRID.rows - 1; r >= 0; r--) {
        if (!removeSet.has(tiles[r][c])) col.push(tiles[r][c].color);
      }
      while (col.length < GRID.rows) col.push(Math.floor(Math.random() * colors.length));
      for (let r = GRID.rows - 1; r >= 0; r--) {
        const idx = GRID.rows - 1 - r;
        tiles[r][c].color = col[idx];
        // recreate mesh color
        const old = tiles[r][c].mesh;
        const gem = createGem(tiles[r][c].color);
        const x = (c - (GRID.cols - 1) / 2) * GRID.spacing;
        const z = (r - (GRID.rows - 1) / 2) * GRID.spacing;
        gem.position.set(x, 0, z);
        tiles[r][c].mesh = gem;
        scene.add(gem);
        if (old) old.geometry && old.geometry.dispose && old.geometry.dispose();
      }
    }
    // after removal, check for new matches
    setTimeout(() => {
      const nextMatches = findAllMatches();
      if (nextMatches.length > 0) removeMatches(nextMatches);
    }, 300);
  }

  function animate() {
    requestAnimationFrame(animate);
    scene.rotation.y += 0.0005;
    renderer.render(scene, camera);
  }

  window.addEventListener('resize', () => {
    camera.aspect = window.innerWidth / window.innerHeight;
    camera.updateProjectionMatrix();
    renderer.setSize(window.innerWidth, window.innerHeight);
  });

})();
