// Shared engine skeleton for platform web-arcade
// Responsibilities:
// - Provide a loader for OBJ/GLTF assets
// - Expose a simple API: Engine.loadModel(url).then(mesh)
// - Provide theme toggles and asset cache

window.PlatformEngine = (function(){
  const cache = new Map();

  function loadOBJ(url){
    if (cache.has(url)) return Promise.resolve(cache.get(url).clone());
    return new Promise((resolve, reject)=>{
      if (!window.THREE || !window.THREE.OBJLoader) return reject(new Error('Three.js/OBJLoader required'));
      const loader = new THREE.OBJLoader();
      loader.load(url, (obj)=>{
        cache.set(url, obj);
        resolve(obj.clone());
      }, undefined, reject);
    });
  }

  return {
    loadModel(url){
      // naive dispatch by extension
      if (url.endsWith('.obj')) return loadOBJ(url);
      return Promise.reject(new Error('Unsupported model format'));
    },
    clearCache(){ cache.clear(); }
  };
})();
