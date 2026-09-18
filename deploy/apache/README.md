# Apache deployment

Apache HTTPD is the public edge. Monolith listens privately on `127.0.0.1:65000`
and Apache terminates TLS and proxies HTTP/WebSocket traffic.

Enable the required Apache modules:

```bash
sudo a2enmod proxy proxy_http proxy_wstunnel headers ssl rewrite
sudo a2ensite monolith.conf
sudo systemctl reload apache2
```

Replace `monolith.example.com` with the Azure DNS name and provide a valid
certificate. Keep port 65000 private; expose only ports 80 and 443 at the edge.

For Azure App Service/Container Apps, use the platform ingress instead of this
file. This configuration is for an Apache host in the Azure network.
