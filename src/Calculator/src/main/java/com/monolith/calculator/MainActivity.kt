package com.monolith.calculator

import android.os.Bundle
import androidx.appcompat.app.AppCompatActivity
import com.monolith.calculator.databinding.ActivityMainBinding

class MainActivity : AppCompatActivity() {

    private lateinit var binding: ActivityMainBinding
    private var currentInput = ""
    private var result = 0.0
    private var pendingOperation = ""

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        binding = ActivityMainBinding.inflate(layoutInflater)
        setContentView(binding.root)

        setupNumberButtons()
        setupOperationButtons()
    }

    private fun setupNumberButtons() {
        val numberButtons = listOf(
            binding.btn0, binding.btn1, binding.btn2, binding.btn3,
            binding.btn4, binding.btn5, binding.btn6, binding.btn7,
            binding.btn8, binding.btn9
        )

        for (button in numberButtons) {
            button.setOnClickListener {
                currentInput += button.text.toString()
                binding.tvDisplay.text = currentInput
            }
        }

        binding.btnClear.setOnClickListener {
            currentInput = ""
            result = 0.0
            pendingOperation = ""
            binding.tvDisplay.text = "0"
        }
    }

    private fun setupOperationButtons() {
        binding.btnAdd.setOnClickListener { performOperation("+") }
        binding.btnSubtract.setOnClickListener { performOperation("-") }
        binding.btnMultiply.setOnClickListener { performOperation("*") }
        binding.btnDivide.setOnClickListener { performOperation("/") }

        binding.btnEquals.setOnClickListener {
            calculateResult()
            pendingOperation = ""
        }
    }

    private fun performOperation(operation: String) {
        if (currentInput.isNotEmpty()) {
            calculateResult()
            pendingOperation = operation
            currentInput = ""
        }
    }

    private fun calculateResult() {
        if (currentInput.isNotEmpty()) {
            val value = currentInput.toDouble()
            when (pendingOperation) {
                "+" -> result += value
                "-" -> result -= value
                "*" -> result *= value
                "/" -> if (value != 0.0) result /= value else result = 0.0
                else -> result = value
            }
            currentInput = ""
            binding.tvDisplay.text = result.toString()
        }
    }
}
