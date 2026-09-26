plugins {
    id("com.android.library")
    id("org.jetbrains.kotlin.android")
}

android {
    namespace = "com.monolith.android.gamehub"
    compileSdk = Versions.compileSdk

    defaultConfig {
        minSdk = Versions.minSdk
        targetSdk = Versions.targetSdk
    }

    buildTypes {
        release {
            isMinifyEnabled = false
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
