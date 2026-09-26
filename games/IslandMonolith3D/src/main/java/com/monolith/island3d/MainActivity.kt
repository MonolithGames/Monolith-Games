package com.monolith.island3d

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectDragGestures
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.unit.dp
import com.google.android.gms.ads.MobileAds
import kotlinx.coroutines.delay

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        MobileAds.initialize(this) {}
        setContent {
            Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                IslandSurvivalScreen()
            }
        }
    }
}

@Composable
fun IslandSurvivalScreen() {
    var wood by remember { mutableIntStateOf(12) }
    var stone by remember { mutableIntStateOf(8) }
    var food by remember { mutableIntStateOf(100) }
    var shelterBuilt by remember { mutableStateOf(false) }
    var playerX by remember { mutableFloatStateOf(400f) }
    var playerY by remember { mutableFloatStateOf(600f) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(1000)
            if (food > 0) food -= 1
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .pointerInput(Unit) {
                detectDragGestures { _, dragAmount ->
                    playerX = (playerX + dragAmount.x).coerceIn(100f, 900f)
                    playerY = (playerY + dragAmount.y).coerceIn(200f, 1400f)
                }
            }
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            drawRect(color = Color(0xFF0284c7)) // Tropical Ocean

            // Island sand shore
            drawCircle(color = Color(0xFFfde047), radius = 380f, center = Offset(500f, 800f))

            // Lush island palm foliage
            drawCircle(color = Color(0xFF15803d), radius = 220f, center = Offset(500f, 800f))

            // Shelter
            if (shelterBuilt) {
                drawRect(color = Color(0xFFb45309), topLeft = Offset(450f, 750f), size = androidx.compose.ui.geometry.Size(100f, 100f))
            }

            // Player Explorer
            drawCircle(color = Color(0xFFe11d48), radius = 30f, center = Offset(playerX, playerY))
        }

        Column(modifier = Modifier.padding(24.dp).align(Alignment.TopStart)) {
            Text("Island Monolith 3D", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Spacer(modifier = Modifier.height(4.dp))
            Text("Wood: $wood | Stone: $stone | Hunger: $food%", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Column(modifier = Modifier.padding(24.dp).align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(10.dp)) {
            Row(horizontalArrangement = Arrangement.spacedBy(12.dp)) {
                Button(onClick = { wood += 5; stone += 3 }) { Text("Forage Island (+5 Wood, +3 Stone)") }
                Button(onClick = { food = (food + 20).coerceAtMost(100) }) { Text("Eat Coconuts (+20 Food)") }
            }
            OutlinedButton(
                onClick = { if (wood >= 10 && stone >= 5) { wood -= 10; stone -= 5; shelterBuilt = true } },
                modifier = Modifier.fillMaxWidth()
            ) {
                Text(if (shelterBuilt) "Island Shelter Established" else "Build Shelter (10 Wood, 5 Stone)")
            }
        }
    }
}
