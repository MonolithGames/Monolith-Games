package com.monolith.aetherdrift

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
    var score by remember { mutableStateOf(0) }
    var coins by remember { mutableStateOf(100) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("AETHER DRIFT", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Score: $score | Coins: $coins", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { score += 10 }) {
            Text("Play / Tap to Boost")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { coins += 50 }) {
            Text("Watch Rewarded Ad (+50 Coins)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { coins += 500 }) {
            Text("Buy 500 Coins ($0.99 IAP)")
        }
    }
}
