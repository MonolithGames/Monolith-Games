#!/bin/sh
set -eu

output_dir=${1:-.}
count=${2:-1000000}

case "$count" in
    ''|*[!0-9]*)
        printf '%s\n' 'count must be a non-negative integer' >&2
        exit 2
        ;;
esac

mkdir -p "$output_dir"
index=0
while [ "$index" -lt "$count" ]; do
    file="$output_dir/$index.c"
    if [ ! -e "$file" ]; then
        printf '/* Generated Monolith source file %s.c. */\n' "$index" > "$file"
    fi
    index=$((index + 1))
done

printf 'Generated files %s through %s in %s\n' 0 $((count - 1)) "$output_dir"
