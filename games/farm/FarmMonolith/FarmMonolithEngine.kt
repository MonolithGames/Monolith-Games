package com.monolith.farm

data class CropPlot(
    val id: Int,
    var seedType: String = "EMPTY",
    var growthProgress: Int = 0,
    var isWatered: Boolean = false
)

class FarmMonolithEngine {
    var gold: Long = 1000
    var farmLevel: Int = 1
    val plots = MutableList(6) { id -> CropPlot(id = id) }

    fun plant(plotId: Int, seed: String) {
        if (plotId in plots.indices) {
            plots[plotId] = CropPlot(id = plotId, seedType = seed, growthProgress = 10)
            println("[FARM ENGINE] Planted $seed in Plot #$plotId")
        }
    }

    fun water(plotId: Int) {
        if (plotId in plots.indices) {
            val plot = plots[plotId]
            plot.growthProgress += 30
            plot.isWatered = true
            if (plot.growthProgress >= 100) {
                plot.growthProgress = 100
            }
            println("[FARM ENGINE] Watered Plot #$plotId. Progress: ${plot.growthProgress}%")
        }
    }

    fun harvest(plotId: Int): Long {
        if (plotId in plots.indices && plots[plotId].growthProgress >= 100) {
            val crop = plots[plotId].seedType
            val reward = when (crop) {
                "WHEAT" -> 50L
                "CORN" -> 120L
                "PUMPKIN" -> 300L
                else -> 10L
            }
            gold += reward
            plots[plotId] = CropPlot(id = plotId)
            println("[FARM ENGINE] Harvested $crop from Plot #$plotId for +$$reward Gold! Total Gold: $gold")
            return reward
        }
        return 0L
    }
}
