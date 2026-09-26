package com.monolith.incorporated.corpneuralnetai

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
                CorpGameScreen()
            }
        }
    }
}

@Composable
fun CorpGameScreen() {
    var revenue by remember { mutableStateOf(84.2) } // Millions
    var efficiency by remember { mutableStateOf(99.1f) }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Column(modifier = Modifier.align(Alignment.TopStart)) {
            Text("CorpNeuralNetAI (Monolith Corp)", style = Modifier.??? - if needed... TextStyle(), color = Color.White)
            Spacer(modifier = Modifier.height(8.dp))
            Text("Quarterly Revenue: ${revenue}M USD", style = MaterialTheme.typography.headlineSmall, color = Color(0xFF4ade80))
            Text("Operational Efficiency: ${efficiency}%", style = MaterialTheme.typography.bodyMedium, color = Color(0xFF60a5fa))
        }

        Column(modifier = Modifier.align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Button(onClick = { revenue += 5.0 }, modifier = Modifier.fillMaxWidth()) {
                Text("Optimize Global Assets (+)")
            }
        }
    }
}

