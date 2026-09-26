package com.monolithgames.calculator

import java.math.BigDecimal
import java.math.MathContext
import java.math.RoundingMode

object CalculatorEngine {

    fun evaluate(expression: String): String {
        if (expression.isBlank()) return "0"
        return try {
            val clean = expression.replace("×", "*").replace("÷", "/")
            val result = parseAndEvaluate(clean)
            formatResult(result)
        } catch (e: ArithmeticException) {
            "Cannot divide by zero"
        } catch (e: Exception) {
            "Error"
        }
    }

    private fun parseAndEvaluate(expr: String): BigDecimal {
        val tokens = tokenize(expr)
        return evalTokens(tokens)
    }

    private fun tokenize(expr: String): List<String> {
        val list = mutableListOf<String>()
        var i = 0
        while (i < expr.length) {
            val c = expr[i]
            when {
                c.isDigit() || c == '.' -> {
                    val sb = StringBuilder()
                    while (i < expr.length && (expr[i].isDigit() || expr[i] == '.')) {
                        sb.append(expr[i])
                        i++
                    }
                    list.add(sb.toString())
                    continue
                }
                c in "+-*/%" -> {
                    list.add(c.toString())
                }
            }
            i++
        }
        return list
    }

    private fun evalTokens(tokens: List<String>): BigDecimal {
        if (tokens.isEmpty()) return BigDecimal.ZERO
        var current = BigDecimal(tokens[0])
        var i = 1
        while (i < tokens.size - 1) {
            val op = tokens[i]
            val next = BigDecimal(tokens[i + 1])
            current = when (op) {
                "+" -> current.add(next)
                "-" -> current.subtract(next)
                "*" -> current.multiply(next)
                "/" -> {
                    if (next.compareTo(BigDecimal.ZERO) == 0) throw ArithmeticException("Division by zero")
                    current.divide(next, MathContext(10, RoundingMode.HALF_UP))
                }
                "%" -> current.remainder(next)
                else -> current
            }
            i += 2
        }
        return current
    }

    private fun formatResult(value: BigDecimal): String {
        val stripped = value.stripTrailingZeros()
        return stripped.toPlainString()
    }
}
