package com.monolith.apexdominion

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
    var territory by remember { mutableStateOf(12) }
    var tribute by remember { mutableStateOf(1000) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("APEX DOMINION", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Territory: $territory Zones | Tribute: $tribute", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { territory += 2 }) {
            Text("Conquer Province")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { tribute += 250 }) {
            Text("Watch Ad (+250 Tribute)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { tribute += 5000 }) {
            Text("Emperor Pack ($4.99 IAP)")
        }
    }
}
