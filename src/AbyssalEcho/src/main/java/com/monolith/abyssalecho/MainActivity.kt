package com.monolith.abyssalecho

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.google.android.gms.ads.MobileAds

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        MobileAds.initialize(this) {}
        setContent {
            Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                GameDashboard()
            }
        }
    }
}

@Composable
fun GameDashboard() {
    var depth by remember { mutableStateOf(1200) }
    var sonarSignals by remember { mutableStateOf(150) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("ABYSSAL ECHO", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Depth: $depth M | Signals: $sonarSignals", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { depth += 300 }) {
            Text("Dive Deeper")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { sonarSignals += 50 }) {
            Text("Watch Ad (+50 Signals)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { sonarSignals += 800 }) {
            Text("Submarine Upgrade ($0.99 IAP)")
        }
    }
}
