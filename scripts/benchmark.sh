#!/usr/bin/env bash
# Search endpoint latency benchmark.
# Hard ceiling defined by the case: 400ms average response time.
# Usage:
#   bash scripts/benchmark.sh [base-url]

set -euo pipefail
BASE_URL="${1:-http://localhost:5114}"
OUT=$(mktemp)

queries=(
  "fatura"
  "fatura+mart"
  "sözleşme+abc"
  "yazılım+teklif"
  "bütçe+rapor"
  "personel"
  "lojistik"
  "elektrik"
)

# Warm-up
for q in "${queries[@]}"; do
  curl -s -o /dev/null "$BASE_URL/api/documents?q=$q&pageSize=10" || true
done

echo "Running 200 requests across ${#queries[@]} queries..."
for i in $(seq 1 200); do
  q=${queries[$((RANDOM % ${#queries[@]}))]}
  curl -s -o /dev/null -w "%{time_total}\n" "$BASE_URL/api/documents?q=$q&pageSize=10"
done > "$OUT"

sort -n "$OUT" | awk '
{
  arr[NR] = $1 * 1000
}
END {
  n = NR
  s = 0
  for (i = 1; i <= n; i++) s += arr[i]
  avg = s / n
  p50 = arr[int(n * 0.50)]
  p95 = arr[int(n * 0.95)]
  p99 = arr[int(n * 0.99)]
  printf "n=%d  min=%.1fms  avg=%.1fms  p50=%.1fms  p95=%.1fms  p99=%.1fms  max=%.1fms\n", n, arr[1], avg, p50, p95, p99, arr[n]
  if (avg > 400) {
    print "FAIL: average exceeds 400ms ceiling"
    exit 1
  }
  if (p95 > 400) {
    print "WARN: p95 exceeds 400ms (avg ok)"
  }
  print "OK: 400ms average ceiling honored"
}'

rm -f "$OUT"
