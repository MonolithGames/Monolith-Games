package com.monolith.arcade.arcadezenith

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
                ArcadeGameScreen()
            }
        }
    }
}

@Composable
fun ArcadeGameScreen() {
    var posX by remember { mutableStateOf(500f) }
    var score by remember { mutableStateOf(0) }
    var coins by remember { mutableStateOf(150) }
    var targetY by remember { mutableStateOf(0f) }
    var targetX by remember { mutableStateOf(400f) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(16)
            targetY += 8f
            if (targetY > 2000f) {
                targetY = 0f
                targetX = (Math.random() * 900).toFloat()
                score += 10
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .pointerInput(Unit) {
                detectDragGestures { _, dragAmount ->
                    posX = (posX + dragAmount.x).coerceIn(60f, 1000f)
                }
            }
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            drawRect(color = Color(0xFF0f172a))
            drawCircle(color = Color(0xFF38bdf8), radius = 45f, center = Offset(posX, size.height - 250f))
            drawCircle(color = Color(0xFF4ade80), radius = 35f, center = Offset(targetX, targetY))
        }

        Column(modifier = Modifier.padding(24.dp).align(Alignment.TopStart)) {
            Text("ArcadeZenith", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Text("Score: $score | Coins: $coins", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Row(
            modifier = Modifier.padding(24.dp).align(Alignment.BottomCenter),
            horizontalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Button(onClick = { coins += 50 }) { Text("Watch Ad (+50)") }
            OutlinedButton(onClick = { coins += 500 }) { Text("Buy Coins (.99)") }
        }
    }
}
