# Monolith-Games Enterprise Architecture (Microsoft Standard)

This document outlines the multi-module architecture, game hub integration, core Android libraries, C/C++/C# engines, backend microservices, and CI/CD pipelines.

## Multi-Module Android Layout (`/android`)
- **launcher**: Main application entry point and intent router.
- **gamehub**: Unified game selection hub, storefront, and launcher dashboard.
- **core**: Shared application utilities, logging, and base architecture components.
- **ui**: Design system, Material 3 themes, and Compose UI components.
- **networking**: Retrofit/OkHttp clients for REST & WebSocket communication with backend services.
- **analytics**: Event telemetry and performance tracking.
- **authentication**: Credential Manager and OAuth2 integration.
- **games**: Individual game modules (`project-alpha`, `project-beta`, `project-gamma`).
- **buildSrc**: Gradle build conventions and dependency management.
