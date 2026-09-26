package com.monolith.kineticszero

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.Canvas
import androidx.compose.foundation.gestures.detectTapGestures
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
                PhysicsGameScreen()
            }
        }
    }
}

@Composable
fun PhysicsGameScreen() {
    var ballY by remember { mutableStateOf(200f) }
    var velocityY by remember { mutableStateOf(0f) }
    var score by remember { mutableStateOf(0) }
    var energy by remember { mutableStateOf(100) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(16)
            velocityY += 0.8f // Gravity
            ballY += velocityY
            if (ballY > 1800f) {
                ballY = 1800f
                velocityY = 0f
            }
        }
    }

    Box(
        modifier = Modifier
            .fillMaxSize()
            .pointerInput(Unit) {
                detectTapGestures {
                    velocityY = -22f // Jump / impulse
                    score += 5
                }
            }
    ) {
        Canvas(modifier = Modifier.fillMaxSize()) {
            drawRect(color = Color(0xFF0a0a0a))
            drawCircle(color = Color(0xFF4ade80), radius = 45f, center = Offset(size.width / 2f, ballY))
        }

        Column(modifier = Modifier.padding(24.dp).align(Alignment.TopStart)) {
            Text("Score: $score", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Text("Energy: $energy", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Row(
            modifier = Modifier.padding(24.dp).align(Alignment.BottomCenter),
            horizontalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            Button(onClick = { energy += 50 }) { Text("Ad (+50)") }
            OutlinedButton(onClick = { energy += 500 }) { Text("Upgrade") }
        }
    }
}
