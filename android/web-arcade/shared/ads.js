// Stub ad manager
window.Ads = (function(){
  return {
    showInterstitial(){ console.log('Ads.showInterstitial called'); },
    showBanner(){ console.log('Ads.showBanner called'); }
  };
})();
