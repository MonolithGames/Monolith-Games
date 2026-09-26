package com.monolith.voidstrikers

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
                SpaceShooterScreen()
            }
        }
    }
}

@Composable
fun SpaceShooterScreen() {
    var playerX by remember { mutableStateOf(500f) }
    var score by remember { mutableStateOf(0) }
    var crystals by remember { mutableStateOf(300) }
    var alienY by remember { mutableStateOf(100f) }
    var alienX by remember { mutableStateOf(400f) }
    var laserY by remember { mutableStateOf(-100f) }
    var laserX by remember { mutableStateOf(0f) }
    var isFiring by remember { mutableStateOf(false) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(16)
            alienY += 4f
            if (alienY > 2000f) {
                alienY = 0f
                alienX = (Math.random() * 900).toFloat()
            }
            if (isFiring) {
                laserY -= 30f
                if (laserY < 0f) isFiring = false
                if (Math.abs(laserX - alienX) < 60f && Math.abs(laserY - alienY) < 60f) {
                    score += 100
                    alienY = 0f
                    alienX = (Math.random() * 900).toFloat()
                    isFiring = false
                }
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .pointerInput(Unit) {
                detectDragGestures { _, dragAmount ->
                    playerX = (playerX + dragAmount.x).coerceIn(60f, 1000f)
                }
            }
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            drawRect(color = Color(0xFF030712))

            drawCircle(color = Color(0xFF38bdf8), radius = 45f, center = Offset(playerX, size.height - 250f))
            drawCircle(color = Color(0xFFf43f5e), radius = 40f, center = Offset(alienX, alienY))

            if (isFiring) {
                drawRect(color = Color(0xFF4ade80), topLeft = Offset(laserX - 6f, laserY), size = androidx.compose.ui.geometry.Size(12f, 40f))
            }
        }

        Column(modifier = Modifier.padding(24.dp).align(Alignment.TopStart)) {
            Text("Score: $score", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Text("Crystals: $crystals", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Row(
            modifier = Modifier.padding(24.dp).align(Alignment.BottomCenter),
            horizontalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Button(onClick = {
                if (!isFiring) {
                    laserX = playerX
                    laserY = 1600f
                    isFiring = true
                }
            }) {
                Text("FIRE LASER")
            }
            Button(onClick = { crystals += 100 }) {
                Text("Ad (+100)")
            }
            OutlinedButton(onClick = { crystals += 1000 }) {
                Text("Laser Pack")
            }
        }
    }
}
