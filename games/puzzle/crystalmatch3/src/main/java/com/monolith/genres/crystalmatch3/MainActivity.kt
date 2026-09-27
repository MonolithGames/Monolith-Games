package com.monolith.genres.crystalmatch3

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

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        MobileAds.initialize(this) {}
        setContent {
            Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                GenreGameScreen()
            }
        }
    }
}

@Composable
fun GenreGameScreen() {
    var score by remember { mutableIntStateOf(0) }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp), contentAlignment = Alignment.Center) {
        Column(horizontalAlignment = Alignment.CenterHorizontally, verticalArrangement = Arrangement.spacedBy(16.dp)) {
            Text("CrystalMatch3", style = MaterialTheme.typography.headlineLarge, color = Color.White)
            Text("Score: $score", style = MaterialTheme.typography.titleMedium, color = Color(0xFF4ade80))
            Button(onClick = { score += 10 }) {
                Text("Interact / Tap")
            }
        }
    }
}
