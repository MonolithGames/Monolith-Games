# Monolith Calculator (Product 001)

A production-oriented Android calculator application built with Kotlin, Jetpack Compose, Material 3, Google Mobile Ads SDK (AdMob), Google UMP SDK, and Google Play Billing Library.

## Project Structure
- **Package**: `com.monolithgames.calculator`
- **Architecture**: MVVM with `CalculatorEngine` (using `BigDecimal` for precise arithmetic), `CalculatorViewModel`, and Jetpack Compose UI.
- **Monetization**: Adaptive bottom banner ad + `$0.99` `remove_ads` In-App Purchase.

## Building
```bash
cd src/Calculator
./gradlew assembleDebug
```
To generate the production release bundle for Google Play Console:
```bash
./gradlew bundleRelease
```
