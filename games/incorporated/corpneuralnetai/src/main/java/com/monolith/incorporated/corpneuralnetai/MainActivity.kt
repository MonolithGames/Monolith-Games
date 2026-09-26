package com.monolith.incorporated.corpneuralnetai

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
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
    var revenue by remember { mutableDoubleStateOf(215.4) }
    var efficiency by remember { mutableFloatStateOf(99.8f) }
    var parameterCount by remember { mutableLongStateOf(175000000000L) } // 175B

    LaunchedEffect(Unit) {
        while(true) {
            delay(2000)
            efficiency = (99f + (Math.random() * 0.95f)).toFloat()
        }
    }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Column(modifier = Modifier.align(Alignment.TopStart)) {
            Text("CorpNeuralNetAI (Monolith Corp)", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Spacer(modifier = Modifier.height(12.dp))
            Text("Quarterly Revenue: \$${String.format("%.2f", revenue)}M USD", style = MaterialTheme.typography.headlineSmall, color = Color(0xFF4ade80))
            Text("Neural Cluster Efficiency: ${String.format("%.1f", efficiency)}%", style = MaterialTheme.typography.bodyLarge, color = Color(0xFF60a5fa))
            Text("AGI Model Parameters: ${parameterCount / 1000000000L} Billion Params", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Column(modifier = Modifier.align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Button(onClick = { revenue += 12.0 }, modifier = Modifier.fillMaxWidth()) {
                Text("Deploy AI Enterprise API (+\$12.0M Revenue)")
            }
            OutlinedButton(onClick = { parameterCount += 25000000000L }, modifier = Modifier.fillMaxWidth()) {
                Text("Scale AGI Compute Cluster (+25B Params)")
            }
        }
    }
}
