package com.monolithgames.calculator

import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.lifecycle.ViewModel

class CalculatorViewModel : ViewModel() {
    var expression by mutableStateOf("")
        private set

    var result by mutableStateOf("0")
        private set

    var history = mutableStateListOf<String>()
        private set

    var adsRemoved by mutableStateOf(false)

    fun onButtonClick(label: String) {
        when (label) {
            "C" -> {
                expression = ""
                result = "0"
            }
            "⌫" -> {
                if (expression.isNotEmpty()) {
                    expression = expression.dropLast(1)
                }
            }
            "=" -> {
                if (expression.isNotBlank()) {
                    val eval = CalculatorEngine.evaluate(expression)
                    result = eval
                    history.add(0, "$expression = $eval")
                }
            }
            "+/-" -> {
                if (expression.startsWith("-")) {
                    expression = expression.substring(1)
                } else {
                    expression = "-$expression"
                }
            }
            else -> {
                expression += label
            }
        }
    }

    fun clearHistory() {
        history.clear()
    }
}
