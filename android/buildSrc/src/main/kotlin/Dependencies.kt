object Versions {
    const val compileSdk = 34
    const val minSdk = 24
    const val targetSdk = 34
    const val versionCode = 1
    const val versionName = "1.0.0"
    
    const val coreKtx = "1.12.0"
    const val lifecycle = "2.7.0"
    const val activityCompose = "1.8.2"
    const val composeBom = "2024.02.00"
    const val billingClient = "6.1.0"
    const val playServicesAds = "22.6.0"
    const val userMessagingPlatform = "2.2.0"
}

object Libs {
    const val coreKtx = "androidx.core:core-ktx:${Versions.coreKtx}"
    const val lifecycleRuntime = "androidx.lifecycle:lifecycle-runtime-ktx:${Versions.lifecycle}"
    const val activityCompose = "androidx.activity:activity-compose:${Versions.activityCompose}"
    const val composeBom = "androidx.compose:compose-bom:${Versions.composeBom}"
    const val composeUi = "androidx.compose.ui:ui"
    const val composeGraphics = "androidx.compose.ui:ui-graphics"
    const val composePreview = "androidx.compose.ui:ui-tooling-preview"
    const val material3 = "androidx.compose.material3:material3"
    
    // Monetization & Ads
    const val billingClient = "com.android.billingclient:billing-ktx:${Versions.billingClient}"
    const val playServicesAds = "com.google.android.gms:play-services-ads:${Versions.playServicesAds}"
    const val userMessagingPlatform = "com.google.android.ump:user-messaging-platform:${Versions.userMessagingPlatform}"
}
