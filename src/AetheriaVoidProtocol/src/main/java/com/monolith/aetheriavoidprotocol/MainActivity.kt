package com.monolith.aetheriavoidprotocol

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
                InteractiveGameScreen()
            }
        }
    }
}

@Composable
fun InteractiveGameScreen() {
    var playerX by remember { mutableStateOf(500f) }
    var score by remember { mutableStateOf(0) }
    var crystals by remember { mutableStateOf(250) }
    var obstacleY by remember { mutableStateOf(0f) }
    var obstacleX by remember { mutableStateOf(300f) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(16)
            obstacleY += 12f
            if (obstacleY > 2000f) {
                obstacleY = 0f
                obstacleX = (Math.random() * 800).toFloat()
                score += 10
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
            drawRect(color = Color(0xFF080808))
            drawCircle(color = Color(0xFF60a5fa), radius = 40f, center = Offset(playerX, size.height - 200f))
            drawCircle(color = Color(0xFFf472b6), radius = 30f, center = Offset(obstacleX, obstacleY))
        }

        Column(
            modifier = Modifier.padding(24.dp).align(Alignment.TopStart)
        ) {
            Text("Score: $score", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Text("Crystals: $crystals", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Row(
            modifier = Modifier.padding(24.dp).align(Alignment.BottomCenter),
            horizontalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Button(onClick = { crystals += 50 }) {
                Text("Ad (+50)")
            }
            OutlinedButton(onClick = { crystals += 500 }) {
                Text("Buy Pack")
            }
        }
    }
}
