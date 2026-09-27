Platform Web Arcade — structure

This folder holds the platform-level web arcade intended to be packaged with the platform services.

Structure:
- android/   -> Android-targeted HTML games and assets
- windows/   -> Windows/desktop-targeted HTML games and wrappers
- web/       -> Generic web-playable versions (hosted in browser)
- shared/    -> Shared engine, loaders, CSS, and utilities used by all games
- docs/      -> Documentation and design notes (this file)

How to use
- Place per-game folders under the appropriate runtime folder (e.g., android/ or web/).
- Each game should expose an index.html and any assets (models/, scripts/, styles/).
- The server maps this folder at /platform/web-arcade when the admin-dashboard app is running.

Development
- Implement shared utilities in shared/ (loader, asset cache, theme toggles).
- Keep game metadata in games/metadata.json and maintain slugs consistent with store listings.
