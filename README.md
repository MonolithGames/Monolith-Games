# Monolith Games

**Founder & Lead Architect**: Mike  
**Studio**: Monolith Games Inc.  

Welcome to the official repository for Monolith Games. Engineered by Mike, this platform adheres to elite A+ enterprise architecture standards, cleanly separating games, platform infrastructure, web applications, branding, legal, and tooling.

## Repository Directory Map

| Directory | Description |
| :--- | :--- |
| **`android/`** | Mobile applications and Android launchers |
| **`games/`** | Pure game titles (*VoidStrikers*, *IslandMonolith3D*, *Arcade*, *Enterprise*, *Incorporated*, *Genres*) |
| **`platform/`** | Monolith infrastructure (`launcher/`, `store/`, `telemetry/`, `editor/`, `publishing/`, `shared/`) |
| **`web/`** | Company websites, `MonolithGames.com`, `Auora.com`, and `I.com` portals |
| **`backend/`** | Cloud microservices, auth, and telemetry backends |
| **`assets/`** | Global textures, audio, models, and UI art assets |
| **`branding/`**| Company logos, icons, marketing art, and store assets |
| **`legal/`** | Trademarks, licenses, privacy policies, and terms |
| **`docs/`** | Architecture designs, decisions, game design specs, and API docs |
| **`roadmap/`** | Project milestones and release planning |
| **`scripts/`** | Build, automation, and deployment scripts |
| **`tools/`** | Internal asset pipelines, publishing tools, and build utilities |
| **`config/`** | Global configuration profiles and environment templates |
| **`third-party/`**| External dependencies and submodule integrations |
| **`.github/`** | CI/CD GitHub Action workflows and templates |

## Automated repository hygiene changes applied

The following conservative changes were applied automatically by the preparatory script to reduce machine-specific configuration and improve portability:

- Replaced user-specific absolute Windows paths in `web/admin-dashboard/Program.cs` and `platform/shared/MonolithFinancial/main.c` with repository-relative fallbacks.
- Appended Android/Gradle/IDE/Visual Studio ignores to `.gitignore` so generated files are not committed.

## Recommended manual upgrades (do in Android Studio / locally with tests)

1. Review and update the Gradle wrapper (`gradle/wrapper/gradle-wrapper.properties`) to a modern stable release compatible with AGP.
2. Update Android Gradle Plugin (AGP) and Kotlin versions in module build scripts; follow the AGP/Gradle/Kotlin compatibility matrix.
3. Open the project in Android Studio, run **Sync Project with Gradle Files**, and resolve any deprecations or compile errors.
4. Add static analysis: detekt and ktlint, and run Android Lint across modules.
5. Add unit tests and instrumentation tests for key modules and configure CI to run them.

## CI

A basic GitHub Actions workflow was added to attempt a Gradle build when a Gradle wrapper is present. It is a starting point; expand it with caching, matrix builds, and signed release artifacts as needed.

## Next steps

- Verify no other user-specific absolute paths remain (search for `C:/Users/` in the repo).
- Confirm SDK/JDK versions required by modules and document them in this README.
- Consider modularizing large game code into feature libraries to speed iteration.
