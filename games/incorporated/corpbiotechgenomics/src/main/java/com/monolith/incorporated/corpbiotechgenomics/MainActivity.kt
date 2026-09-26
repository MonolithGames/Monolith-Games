package com.monolith.incorporated.corpbiotechgenomics

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
import kotlinx.coroutines.delay

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        MobileAds.initialize(this) {}
        setContent {
            Surface(modifier = Modifier.fillMaxSize(), color = MaterialTheme.colorScheme.background) {
                CorpGameScreen()
            }
        }
    }
}

@Composable
fun CorpGameScreen() {
    var revenue by remember { mutableDoubleStateOf(84.2) }
    var efficiency by remember { mutableFloatStateOf(99.1f) }
    var patentCount by remember { mutableIntStateOf(14) }

    LaunchedEffect(Unit) {
        while(true) {
            delay(2000)
            efficiency = (98f + (Math.random() * 1.9f)).toFloat()
        }
    }

    Box(modifier = Modifier.fillMaxSize().padding(24.dp)) {
        Column(modifier = Modifier.align(Alignment.TopStart)) {
            Text("CorpBioTechGenomics (Monolith Corp)", style = MaterialTheme.typography.titleLarge, color = Color.White)
            Spacer(modifier = Modifier.height(12.dp))
            Text("Quarterly Revenue: \$${String.format("%.2f", revenue)}M USD", style = MaterialTheme.typography.headlineSmall, color = Color(0xFF4ade80))
            Text("R&D Efficiency: ${String.format("%.1f", efficiency)}%", style = MaterialTheme.typography.bodyLarge, color = Color(0xFF60a5fa))
            Text("Active Genomic Patents: $patentCount", style = MaterialTheme.typography.bodyMedium, color = Color.LightGray)
        }

        Column(modifier = Modifier.align(Alignment.BottomCenter), verticalArrangement = Arrangement.spacedBy(12.dp)) {
            Button(onClick = { revenue += 5.2 }, modifier = Modifier.fillMaxWidth()) {
                Text("Monetize Genomic Patent (+\$5.2M Revenue)")
            }
            OutlinedButton(onClick = { patentCount += 1 }, modifier = Modifier.fillMaxWidth()) {
                Text("File New Biotech Patent (R&D Expansion)")
            }
        }
    }
}
