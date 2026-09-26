package com.monolith.chronosbreach

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
    var era by remember { mutableStateOf(2077) }
    var chronotokens by remember { mutableStateOf(100) }

    Column(
        modifier = Modifier.fillMaxSize().padding(24.dp),
        verticalArrangement = Arrangement.Center,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        Text("CHRONOS BREACH", style = MaterialTheme.typography.headlineLarge)
        Spacer(modifier = Modifier.height(16.dp))
        Text("Era: $era AD | Tokens: $chronotokens", style = MaterialTheme.typography.titleMedium)
        Spacer(modifier = Modifier.height(32.dp))
        Button(onClick = { era += 50 }) {
            Text("Jump Timeline")
        }
        Spacer(modifier = Modifier.height(16.dp))
        Button(onClick = { chronotokens += 50 }) {
            Text("Watch Ad (+50 Tokens)")
        }
        Spacer(modifier = Modifier.height(16.dp))
        OutlinedButton(onClick = { chronotokens += 600 }) {
            Text("Time Lord Pack ($1.49 IAP)")
        }
    }
}
