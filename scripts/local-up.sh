#!/usr/bin/env sh
set -eu

compose_file=docker-compose.local.yml
if [ -f .env.local ]; then
    export $(sed '/^[[:space:]]*#/d;/^[[:space:]]*$/d;s/[[:space:]]*=[[:space:]]*/=/' .env.local | xargs)
fi

docker compose -f "$compose_file" up --build -d
printf '%s\n' 'Monolith local runtime started on http://127.0.0.1:5187'
