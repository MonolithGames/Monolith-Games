pluginManagement {
    repositories {
        google()
        mavenCentral()
        gradlePluginPortal()
    }
}
dependencyResolutionManagement {
    repositoriesMode.set(RepositoriesMode.FAIL_ON_PROJECT_REPOS)
    repositories {
        google()
        mavenCentral()
    }
}

rootProject.name = "Monolith-Games"
include(":Calculator")
project(":Calculator").projectDir = file("src/Calculator")
include(":NeonBlade")
project(":NeonBlade").projectDir = file("src/NeonBlade")
include(":QuantumGrid")
project(":QuantumGrid").projectDir = file("src/QuantumGrid")
include(":PixelQuest")
project(":PixelQuest").projectDir = file("src/PixelQuest")
include(":EchoRider")
project(":EchoRider").projectDir = file("src/EchoRider")
