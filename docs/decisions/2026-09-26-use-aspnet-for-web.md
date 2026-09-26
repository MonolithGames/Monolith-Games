# Architecture Decision: Use ASP.NET Core & Blazor for Web Platforms

- **Status**: Accepted
- **Date**: 2026-09-26
- **Context**: The Monolith ecosystem requires a high-performance, secure web platform to manage production workflows, display financial intelligence dashboards, host the Monolith Store, and manage exchange integrations.
- **Decision**: Adopt ASP.NET Core with interactive Blazor Server components, Entity Framework Core (SQLite), and WinUI 3 Fluent styling.
- **Consequences**: Enables rapid feature delivery, robust server-side security, encrypted credential storage at rest via Data Protection, and seamless C# code sharing across backend services and tooling.
