package com.monolith.valkyrieascendant

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
    var rank by remember { mutableStateOf("Einherjar") }
    var glory by remember { mutableStateOf(500) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("VALKYRIE ASCENDANT", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Rank: $rank | Glory: $glory", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { glory += 50 }) {
            Text("Claim Battle Glory")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { glory += 150 }) {
            Text("Watch Ad (+150 Glory)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { glory += 2000 }) {
            Text("Valhalla Pass ($2.99 IAP)")
        }
    }
}
