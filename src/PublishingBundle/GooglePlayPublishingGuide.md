# Monolith Games - Google Play Console Publishing Guide

This guide details the step-by-step process for building, signing, and publishing all Monolith Games under the **Monolith Games** developer account.

---

## 1. Prerequisites
- **Google Play Console Developer Account**: Registered under "Monolith Games".
- **Android Studio / Gradle**: JDK 17 and Android SDK 34 installed.
- **Signing Keystore**: `monolith-release-key.jks` generated for signing release builds.

---

## 2. Building Signed App Bundles (`.aab`)
For each game project located in `src/[GameName]/`, run the release bundle command in terminal:

```bash
cd src/[GameName]
./gradlew bundleRelease
```

This generates the production-ready signed Android App Bundle at:
`src/[GameName]/build/outputs/bundle/release/[GameName]-release.aab`

---

## 3. Google Play Console Upload Steps
1. Log in to the [Google Play Console](https://play.google.com/console).
2. Click **Create App** for each game listed in `PublishingManifest.json`.
3. Fill in store details using the metadata provided in `PublishingAssets/GooglePlayStoreListings.md`.
4. Upload the `.aab` bundle file.
5. Submit for Google Play review and immediate publication!
