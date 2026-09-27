export interface TowerSpec {
    id: string;
    type: 'cannon' | 'laser' | 'slow' | 'artillery';
    range: number;
    damage: number;
    fireRate: number; // shots per sec
    cost: number;
    x: number;
    y: number;
}

export class TowerPlacement {
    private towers: TowerSpec[] = [];

    public placeTower(type: 'cannon' | 'laser' | 'slow' | 'artillery', gridX: number, gridY: number, availableGold: number): TowerSpec | null {
        const cost = type === 'laser' ? 150 : (type === 'artillery' ? 200 : 100);
        if (availableGold < cost) {
            console.log(`[TOWER DEFENSE] Insufficient gold to place ${type} tower. Cost: ${cost}, Gold: ${availableGold}`);
            return null;
        }

        const newTower: TowerSpec = {
            id: `tower_${gridX}_${gridY}`,
            type: type,
            range: type === 'artillery' ? 250 : 120,
            damage: type === 'artillery' ? 85 : 25,
            fireRate: type === 'laser' ? 2.5 : 1.0,
            cost: cost,
            x: gridX * 64,
            y: gridY * 64
        };

        this.towers.push(newTower);
        console.log(`[TOWER DEFENSE] Placed ${type} tower at grid (${gridX}, ${gridY}). Remaining Gold: ${availableGold - cost}`);
        return newTower;
    }

    public getTowers(): TowerSpec[] {
        return this.towers;
    }
}
