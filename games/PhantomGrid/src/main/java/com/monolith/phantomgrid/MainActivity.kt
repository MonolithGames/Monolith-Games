package com.monolith.phantomgrid

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
    var node by remember { mutableStateOf(4) }
    var credits by remember { mutableStateOf(500) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("PHANTOM GRID", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Node: $node | Credits: $credits", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { node += 1 }) {
            Text("Infiltrate Node")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { credits += 100 }) {
            Text("Watch Ad (+100 Credits)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { credits += 1200 }) {
            Text("Hacker Toolkit ($0.99 IAP)")
        }
    }
}
