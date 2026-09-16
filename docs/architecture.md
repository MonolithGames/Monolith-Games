# Monolith Architecture

The active product is the server-interactive .NET application in
`web/Monolith.Web`. It exposes the dashboard, authentication, project/job APIs,
SQLite persistence, media storage, and the background pipeline worker.

The `engine`, `visualstudio`, `unity`, and `unreal` directories are deliberately
scaffolded integration boundaries. Their placeholder files document ownership
without claiming that native engine, Unity, Unreal, or Visual Studio builds are
already wired into the web pipeline.

```text
Browser -> Blazor Web App -> authenticated API
                         -> SQLite / App_Data
                         -> pipeline worker
                         -> configured external adapters
```