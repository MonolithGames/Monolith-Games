import { WaveSystem } from '../../frameworks/tower-defense-framework/WaveSystem';
import { TowerPlacement } from '../../frameworks/tower-defense-framework/TowerPlacement';
import { EconomySystem } from '../../frameworks/tower-defense-framework/EconomySystem';

export class RoadblockGame {
    private waves = new WaveSystem();
    private towers = new TowerPlacement();
    private economy = new EconomySystem();

    public init(): void {
        console.log("=== COCOS CREATOR: ROADBLOCK TOWER DEFENSE INIT ===");
        console.log(`Starting Gold: ${this.economy.getGold()} | Base Lives: ${this.economy.getLives()}`);

        // Place initial roadblock tower
        const tower = this.towers.placeTower('cannon', 2, 4, this.economy.getGold());
        if (tower) {
            this.economy.spendGold(tower.cost);
        }

        // Start Wave 1
        const wave1Enemies = this.waves.startNextWave();
        console.log(`[ROADBLOCK] Defending against ${wave1Enemies.length} enemies in Wave ${this.waves.getWaveNumber()}!`);
    }
}

// Auto-run simulation test
const game = new RoadblockGame();
game.init();
