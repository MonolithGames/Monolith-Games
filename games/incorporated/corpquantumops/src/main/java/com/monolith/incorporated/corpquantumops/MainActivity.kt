package com.monolith.incorporated.corpquantumops

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
                IncorporatedGameScreen()
            }
        }
    }
}

@Composable
fun IncorporatedGameScreen() {
    var sharePrice by remember { mutableStateOf(142.50) }
    var marketCap by remember { mutableStateOf(12.4) } // Billions
    var activeSubs by remember { mutableStateOf(450000) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(3000)
            sharePrice += (Math.random() * 2.0 - 0.9);
        }
    }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Column(modifier = Modifier.align(Alignment.TopStart)) {
            Text("CorpQuantumOps (Monolith Interactive Corp)", style = MaterialTheme.typography.titleMedium, color = Color.White)
            Spacer(modifier = Modifier.height(8.dp))
            Text("Share Price: ${String.format(\"%.2f\", sharePrice)}", style = MaterialTheme.typography.headlineMedium, color = Color(0xFF4ade80))
            Text("Market Cap: ${marketCap}B USD", style = MaterialTheme.typography.bodyMedium, color = Color(0xFF60a5fa))
            Text("Active Subscribers: $activeSubs", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Column(modifier = Modifier.align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Button(onClick = { activeSubs += 10000 }, modifier = Modifier.fillMaxWidth()) {
                Text("Expand Global Publishing (+10K Subs)")
            }
            OutlinedButton(onClick = { marketCap += 0.5 }, modifier = Modifier.fillMaxWidth()) {
                Text("Issue Corporate Bonds (.99 Executive Pass)")
            }
        }
    }
}
