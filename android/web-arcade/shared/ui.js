// Simple UI helpers
window.WebUI = (function(){
  function toast(msg){
    const el = document.createElement('div');
    el.textContent = msg;
    el.style.position='fixed';el.style.right='1rem';el.style.bottom='1rem';el.style.background='#222';el.style.color='#fff';el.style.padding='0.5rem 1rem';el.style.borderRadius='6px';el.style.opacity='0.95';
    document.body.appendChild(el);
    setTimeout(()=>el.remove(),2500);
  }
  return { toast };
})();
