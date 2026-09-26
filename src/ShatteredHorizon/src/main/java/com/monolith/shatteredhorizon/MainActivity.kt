package com.monolith.shatteredhorizon

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
    var outpost by remember { mutableStateOf(1) }
    var resources by remember { mutableStateOf(400) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("SHATTERED HORIZON", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Outpost: $outpost | Resources: $resources", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { outpost += 1 }) {
            Text("Expand Colony")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { resources += 100 }) {
            Text("Watch Ad (+100 Resources)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { resources += 1500 }) {
            Text("Colony Mega Pack ($1.99 IAP)")
        }
    }
}
