export interface EnemySpec {
    id: string;
    health: number;
    speed: number;
    goldValue: number;
}

export class WaveSystem {
    private currentWave: number = 0;
    private isWaveActive: boolean = false;

    public startNextWave(): EnemySpec[] {
        this.currentWave++;
        this.isWaveActive = true;
        const enemyCount = this.currentWave * 5 + 5;
        const enemies: EnemySpec[] = [];

        for (let i = 0; i < enemyCount; i++) {
            enemies.push({
                id: `enemy_w${this.currentWave}_${i}`,
                health: 100 + this.currentWave * 25,
                speed: 1.5 + Math.min(this.currentWave * 0.1, 3.0),
                goldValue: 10 + this.currentWave * 2
            });
        }

        console.log(`[TOWER DEFENSE FRAMEWORK] Wave ${this.currentWave} started with ${enemyCount} enemies.`);
        return enemies;
    }

    public getWaveNumber(): number {
        return this.currentWave;
    }
}
