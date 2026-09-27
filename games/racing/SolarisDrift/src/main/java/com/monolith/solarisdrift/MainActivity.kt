package com.monolith.solarisdrift

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
    var speed by remember { mutableStateOf(340) }
    var solarCredits by remember { mutableStateOf(450) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("SOLARIS DRIFT", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Speed: $speed KM/S | Credits: $solarCredits", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { speed += 25 }) {
            Text("Solar Burn")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { solarCredits += 100 }) {
            Text("Watch Ad (+100 Credits)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { solarCredits += 1500 }) {
            Text("Solar Engine ($1.99 IAP)")
        }
    }
}
