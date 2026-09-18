#!/usr/bin/env sh
set -eu

base_url=${MONOLITH_BASE_URL:-http://127.0.0.1:5187}
health=$(curl -fsS "$base_url/health")
manifest=$(curl -fsS "$base_url/manifest.webmanifest")
printf 'Health: %s\n' "$health"
printf 'PWA manifest: available (%s bytes)\n' "$(printf '%s' "$manifest" | wc -c | tr -d ' ')"
