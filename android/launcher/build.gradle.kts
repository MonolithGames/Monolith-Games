plugins {
    id("com.android.application")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.monolith.android.launcher"
    compileSdk = Versions.compileSdk

    defaultConfig {
        applicationId = "com.monolith.android.launcher"
        minSdk = Versions.minSdk
        targetSdk = Versions.targetSdk
        versionCode = Versions.versionCode
        versionName = Versions.versionName
    }

    buildTypes {
        release {
            isMinifyEnabled = true
            proguardFiles(getDefaultProguardFile("proguard-android-optimize.txt"), "proguard-rules.pro")
        }
    }
    compileOptions {
        sourceCompatibility = JavaVersion.VERSION_17
        targetCompatibility = JavaVersion.VERSION_17
    }
    kotlinOptions {
        jvmTarget = "17"
    }
    buildFeatures {
        compose = true
    }
    composeOptions {
        kotlinCompilerExtensionVersion = "1.5.8"
    }
}

dependencies {
    implementation(project(":gamehub"))
    implementation(project(":core"))
    implementation(project(":ui"))
    implementation(Libs.coreKtx)
    implementation(Libs.lifecycleRuntime)
    implementation(Libs.activityCompose)
    implementation(platform(Libs.composeBom))
    implementation(Libs.composeUi)
    implementation(Libs.composeGraphics)
    implementation(Libs.composePreview)
    implementation(Libs.material3)
    
    // Monetization & Ads
    implementation(Libs.billingClient)
    implementation(Libs.playServicesAds)
    implementation(Libs.userMessagingPlatform)
}
