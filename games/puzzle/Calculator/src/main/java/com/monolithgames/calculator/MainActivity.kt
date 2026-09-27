package com.monolithgames.calculator

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.activity.viewModels
import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import com.google.android.gms.ads.MobileAds
import com.google.android.ump.UserMessagingPlatform

class MainActivity : ComponentActivity() {
    private val viewModel: CalculatorViewModel by viewModels()

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)

        val consentInformation = UserMessagingPlatform.getConsentInformation(this)
        consentInformation.requestConsentInfoUpdate(this, null, {}, {})

        MobileAds.initialize(this) {}

        setContent {
            MaterialTheme(colorScheme = darkColorScheme()) {
                Surface(modifier = Modifier.fillMaxSize(), color = Color(0xFF0A0F1D)) {
                    CalculatorScreen(viewModel)
                }
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun CalculatorScreen(viewModel: CalculatorViewModel) {
    var showSettings by remember { mutableStateOf(false) }
    var showHistory by remember { mutableStateOf(false) }

    Column(modifier = Modifier.fillMaxSize().padding(16.dp)) {
        Row(
            modifier = Modifier.fillMaxWidth(),
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text("MONOLITH", color = Color(0xFF4ADE80), fontWeight = FontWeight.Bold, fontSize = 16.sp)
            Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                IconButton(onClick = { showHistory = true }) {
                    Text("📜", fontSize = 18.sp)
                }
                IconButton(onClick = { showSettings = true }) {
                    Text("⚙️", fontSize = 18.sp)
                }
            }
        }

        Column(
            modifier = Modifier
                .fillMaxWidth()
                .weight(1f)
                .background(Color(0xFF111827), RoundedCornerShape(12.dp))
                .padding(20.dp),
            verticalArrangement = Arrangement.Bottom,
            horizontalAlignment = Alignment.End
        ) {
            Text(viewModel.expression, color = Color.Gray, fontSize = 24.sp, textAlign = TextAlign.End)
            Spacer(modifier = Modifier.height(8.dp))
            Text(viewModel.result, color = Color.White, fontSize = 48.sp, fontWeight = FontWeight.Bold, textAlign = TextAlign.End)
        }

        Spacer(modifier = Modifier.height(16.dp))

        val buttons = listOf(
            listOf("C", "+/-", "%", "÷"),
            listOf("7", "8", "9", "×"),
            listOf("4", "5", "6", "-"),
            listOf("1", "2", "3", "+"),
            listOf("0", ".", "⌫", "=")
        )

        Column(modifier = Modifier.fillMaxWidth(), verticalArrangement = Arrangement.spacedBy(10.dp)) {
            for (row in buttons) {
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.spacedBy(10.dp)) {
                    for (label in row) {
                        CalculatorButton(
                            label = label,
                            modifier = Modifier.weight(1f),
                            onClick = { viewModel.onButtonClick(label) }
                        )
                    }
                }
            }
        }

        if (!viewModel.adsRemoved) {
            Spacer(modifier = Modifier.height(12.dp))
            Box(
                modifier = Modifier
                    .fillMaxWidth()
                    .height(50.dp)
                    .background(Color(0xFF1E293B), RoundedCornerShape(8.dp)),
                contentAlignment = Alignment.Center
            ) {
                Text("[ Adaptive Banner Ad - AdMob Active ]", color = Color.Gray, fontSize = 12.sp)
            }
        }
    }

    if (showHistory) {
        ModalBottomSheet(onDismissRequest = { showHistory = false }) {
            Column(modifier = Modifier.fillMaxWidth().padding(24.dp)) {
                Row(modifier = Modifier.fillMaxWidth(), horizontalArrangement = Arrangement.SpaceBetween) {
                    Text("Calculation History", style = MaterialTheme.typography.titleLarge)
                    TextButton(onClick = { viewModel.clearHistory() }) { Text("Clear") }
                }
                Spacer(modifier = Modifier.height(12.dp))
                if (viewModel.history.isEmpty()) {
                    Text("No calculations yet.", color = Color.Gray)
                } else {
                    LazyColumn(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                        items(viewModel.history) { item ->
                            Text(item, color = Color.White, fontSize = 18.sp)
                        }
                    }
                }
            }
        }
    }

    if (showSettings) {
        ModalBottomSheet(onDismissRequest = { showSettings = false }) {
            Column(modifier = Modifier.fillMaxWidth().padding(24.dp), verticalArrangement = Arrangement.spacedBy(16.dp)) {
                Text("Monolith Calculator Settings", style = MaterialTheme.typography.titleLarge)
                HorizontalDivider(color = Color.Gray)
                Button(onClick = { viewModel.adsRemoved = true; showSettings = false }) {
                    Text(if (viewModel.adsRemoved) "Ads Removed (Active)" else "Remove Ads ($0.99 IAP)")
                }
                OutlinedButton(onClick = { viewModel.adsRemoved = true; showSettings = false }) {
                    Text("Restore Purchases")
                }
                Text("Privacy Choices (UMP Consent)", color = Color.LightGray, modifier = Modifier.clickable { })
                Text("Privacy Policy", color = Color.LightGray, modifier = Modifier.clickable { })
                Text("Open Source Licenses", color = Color.LightGray, modifier = Modifier.clickable { })
                Text("Version 1.0.0 (Build 100)", color = Color.Gray, fontSize = 12.sp)
            }
        }
    }
}

@Composable
fun CalculatorButton(label: String, modifier: Modifier, onClick: () -> Unit) {
    val isOperator = label in listOf("÷", "×", "-", "+", "=")
    val isAction = label in listOf("C", "+/-", "%", "⌫")

    val bg = when {
        label == "=" -> Color(0xFF2563EB)
        isOperator -> Color(0xFF1E293B)
        isAction -> Color(0xFF334155)
        else -> Color(0xFF0F172A)
    }

    Box(
        modifier = modifier
            .height(68.dp)
            .background(bg, RoundedCornerShape(12.dp))
            .clickable(onClick = onClick),
        contentAlignment = Alignment.Center
    ) {
        Text(
            text = label,
            fontSize = 24.sp,
            fontWeight = FontWeight.Bold,
            color = if (isOperator) Color(0xFF60A5FA) else Color.White
        )
    }
}
