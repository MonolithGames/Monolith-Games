# Azure CI/CD and Apache edge

The VM is no longer part of the deployment design. The application is built in
GitHub Actions, pushed to Azure Container Registry, and deployed to an Azure
Web App for Containers or Azure Container Apps. Apache HTTPD is used only when
running an Apache edge inside the Azure network.

```text
GitHub Actions
    -> tests + Docker build
    -> Azure Container Registry
    -> protected production deployment
    -> Azure web runtime
    -> optional Apache HTTPD edge
    -> Monolith container :65000
```

## GitHub configuration

Repository secrets:

```text
AZURE_CREDENTIALS
AZURE_CONTAINER_REGISTRY_LOGIN_SERVER
AZURE_CONTAINER_REGISTRY_USERNAME
AZURE_CONTAINER_REGISTRY_PASSWORD
```

Repository variable:

```text
AZURE_WEBAPP_NAME
```

Application secrets should be configured in Azure App Settings or Key Vault,
not GitHub workflow files. Keep `COINBASE_LIVE_TRADING_ENABLED=false`,
`TRADING_KILL_SWITCH=true`, and `COINBASE_WITHDRAWALS_ENABLED=false` until the
trading control plane is independently approved.

## Apache

Use `deploy/apache/monolith.conf` when Apache is the public edge. It forwards
HTTP and Blazor WebSocket traffic to the private container port 65000 and
terminates TLS at Apache. Expose only 80/443 publicly.
