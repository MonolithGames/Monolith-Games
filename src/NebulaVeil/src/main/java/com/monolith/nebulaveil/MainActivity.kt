package com.monolith.nebulaveil

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
    var sector by remember { mutableStateOf(7) }
    var stardust by remember { mutableStateOf(300) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("NEBULA VEIL", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Sector: $sector | Stardust: $stardust", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { sector += 1 }) {
            Text("Warp to Next Sector")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { stardust += 75 }) {
            Text("Watch Ad (+75 Stardust)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { stardust += 1000 }) {
            Text("Nebula Core ($1.99 IAP)")
        }
    }
}
