// Simple local save system
window.SaveSystem = (function(){
  return {
    save(key,obj){ localStorage.setItem(key, JSON.stringify(obj)); },
    load(key){ const v = localStorage.getItem(key); return v?JSON.parse(v):null; }
  };
})();
