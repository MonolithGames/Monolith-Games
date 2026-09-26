package com.monolith.incorporated.corpgloballogistics

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
    var revenue by remember { mutableDoubleStateOf(142.8) }
    var efficiency by remember { mutableFloatStateOf(97.5f) }
    var fleetSize by remember { mutableIntStateOf(320) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(2000)
            efficiency = (96f + (Math.random() * 3.8f)).toFloat()
        }
    }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Column(modifier = Modifier.align(Alignment.TopStart)) {
            Text("CorpGlobalLogistics (Monolith Corp)", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Spacer(modifier = Modifier.height(12.dp))
            Text("Quarterly Revenue: \$${String.format("%.2f", revenue)}M USD", style = MaterialTheme.typography.headlineSmall, color = Color(0xFF4ade80))
            Text("Freight Efficiency: ${String.format("%.1f", efficiency)}%", style = MaterialTheme.typography.bodyLarge, color = Color(0xFF60a5fa))
            Text("Active Freight Vessels: $fleetSize Cargo Ships", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Column(modifier = Modifier.align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Button(onClick = { revenue += 8.5 }, modifier = Modifier.fillMaxWidth()) {
                Text("Optimize Shipping Routes (+\$8.5M Revenue)")
            }
            OutlinedButton(onClick = { fleetSize += 12 }, modifier = Modifier.fillMaxWidth()) {
                Text("Expand Autonomous Fleet (+12 Vessels)")
            }
        }
    }
}
