// Simple analytics stub
window.Analytics = (function(){
  return {
    event(name,props){ console.log('Analytics.event',name,props||{}); }
  };
})();
