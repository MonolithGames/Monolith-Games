#!/usr/bin/env bash
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
monolith_port=16500
sentinel_port=16535
monolith_log="$(mktemp)"
sentinel_log="$(mktemp)"
data_path="$(mktemp -d)"

cleanup() {
    [[ -n "${monolith_pid:-}" ]] && kill "$monolith_pid" 2>/dev/null || true
    [[ -n "${sentinel_pid:-}" ]] && kill "$sentinel_pid" 2>/dev/null || true
    rm -f "$monolith_log" "$sentinel_log"
    rm -rf "$data_path"
}
trap cleanup EXIT

cd "$root"
dotnet build Monolith.sln --no-restore
dotnet run --project tests/Monolith.Tests/Monolith.Tests.csproj --no-build
dotnet run --project tests/Monolith.Sentinel.Tests/Monolith.Sentinel.Tests.csproj --no-build
grep -q 'applicationUrl.*65000' src/Monolith/Properties/launchSettings.json
grep -q '65535' src/Monolith.Sentinel/Program.cs

if git grep -nE '6500[1-9]|650[1-9][0-9]|65[1-4][0-9]{2}|655[0-2][0-9]|6553[0-4]' -- \
    src config docker-compose.local.yml docker-compose.production.yml docker-compose.simulation.yml; then
    echo "Found an intermediate port binding or assignment." >&2
    exit 1
fi

MONOLITH_DATA_PATH="$data_path" dotnet run --project src/Monolith/Monolith.csproj --no-build --no-launch-profile -- \
    --urls "http://127.0.0.1:$monolith_port" >"$monolith_log" 2>&1 &
monolith_pid=$!
    Sentinel__MonolithBaseUrl="http://127.0.0.1:$monolith_port" dotnet run --project src/Monolith.Sentinel/Monolith.Sentinel.csproj --no-build --no-launch-profile -- \
    --urls "http://127.0.0.1:$sentinel_port" >"$sentinel_log" 2>&1 &
sentinel_pid=$!

curl --silent --show-error --fail --retry 60 --retry-all-errors --retry-delay 1 \
    "http://127.0.0.1:$monolith_port/health" >/dev/null
curl --silent --show-error --fail --retry 30 --retry-all-errors --retry-delay 1 \
    "http://127.0.0.1:$sentinel_port/health/live" >/dev/null

for module in data cache events integration analytics; do
    curl --silent --fail "http://127.0.0.1:$monolith_port/api/$module/status" >/dev/null
done
data_response="$(curl --silent --fail --header 'Content-Type: application/json' \
    --data '{"key":"architecture-check","value":{"ok":true}}' \
    "http://127.0.0.1:$monolith_port/api/data/records")"
grep -q '"key":"architecture-check"' <<<"$data_response"
curl --silent --fail "http://127.0.0.1:$monolith_port/api/data/records/architecture-check" >/dev/null
curl --silent --fail --header 'Content-Type: application/json' \
    --data '{"value":{"ok":true},"ttlSeconds":60}' \
    --request PUT "http://127.0.0.1:$monolith_port/api/cache/entries/architecture-check"
curl --silent --fail "http://127.0.0.1:$monolith_port/api/cache/entries/architecture-check" >/dev/null
curl --silent --fail --header 'Content-Type: application/json' \
    --data '{"eventType":"ArchitectureCheck","payload":{"ok":true}}' \
    "http://127.0.0.1:$monolith_port/api/events" >/dev/null
curl --silent --fail "http://127.0.0.1:$monolith_port/api/events/recent?limit=1" >/dev/null
curl --silent --fail "http://127.0.0.1:$monolith_port/api/integration/providers" >/dev/null
curl --silent --fail "http://127.0.0.1:$monolith_port/api/analytics/summary" >/dev/null
curl --silent --fail "http://127.0.0.1:$monolith_port/api/features" | grep -q '"identity":65001'
for route in health/live health/ready status version components; do
    curl --silent --fail "http://127.0.0.1:$sentinel_port/$route" >/dev/null
done
curl --silent --fail "http://127.0.0.1:$sentinel_port/components" | grep -q '"name":"Data"'

    ports_json="$(curl --silent --fail "http://127.0.0.1:$sentinel_port/ports")"
    grep -q '"port":65000' <<<"$ports_json"
    grep -q '"port":65535' <<<"$ports_json"
    grep -q '"availableFeatureIdentities":534' <<<"$ports_json"
    test "$(grep -o '"size":89' <<<"$ports_json" | wc -l)" -eq 6
    features_json="$(curl --silent --fail "http://127.0.0.1:$sentinel_port/features")"
    test "$(grep -o '"port":' <<<"$features_json" | wc -l)" -eq 534
    feature="$(curl --silent --fail "http://127.0.0.1:$sentinel_port/features/65001")"
    grep -q '"featureId":"PLATFORM-001"' <<<"$feature"
    grep -q '"status":"Unallocated"' <<<"$feature"
    category="$(curl --silent --fail "http://127.0.0.1:$sentinel_port/features/category/Data")"
    test "$(grep -o '"port":' <<<"$category" | wc -l)" -eq 89
    status="$(curl --silent --fail "http://127.0.0.1:$sentinel_port/features/status/Unallocated")"
    test "$(grep -o '"port":' <<<"$status" | wc -l)" -eq 534
    test "$(curl --silent --output /dev/null --write-out '%{http_code}' \
        --header 'Content-Type: application/json' \
        --data '{"name":"WeatherCache","description":"Weather caching capability"}' \
        "http://127.0.0.1:$sentinel_port/features/65001/assign")" = 404
test "$(curl --silent --output /dev/null --write-out '%{http_code}' \
    "http://127.0.0.1:$sentinel_port/port/65001")" = 404

echo "Architecture checks passed."