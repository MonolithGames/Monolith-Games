// Simple leaderboard stub
window.Leaderboard = (function(){
  const scores = [];
  return {
    submit(name,score){ scores.push({name,score,ts:Date.now()}); },
    top(n=10){ return scores.sort((a,b)=>b.score-a.score).slice(0,n); }
  };
})();
