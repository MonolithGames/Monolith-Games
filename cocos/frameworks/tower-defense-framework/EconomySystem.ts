export class EconomySystem {
    private gold: number = 300; // Starting Gold
    private lives: number = 20;

    public getGold(): number { return this.gold; }
    public getLives(): number { return this.lives; }

    public addReward(amount: number): void {
        this.gold += amount;
        console.log(`[ECONOMY] Reward added: +${amount} gold. Current Total: ${this.gold}`);
    }

    public spendGold(amount: number): boolean {
        if (this.gold >= amount) {
            this.gold -= amount;
            return true;
        }
        return false;
    }

    public loseLife(): boolean {
        this.lives--;
        console.log(`[ECONOMY] Base breached! Remaining Lives: ${this.lives}`);
        return this.lives <= 0;
    }

    public claimAdReward(): void {
        this.addReward(200); // Rewarded Video Ad bonus
    }
}
