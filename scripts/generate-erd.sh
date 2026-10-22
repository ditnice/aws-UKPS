#!/usr/bin/env bash
# Regenerates documentation/database/ukps-erd.mmd and ukps-erd.html from the EF Core model.
#
# Usage:
#   ./scripts/generate-erd.sh            # regenerate the committed ERD
#   ./scripts/generate-erd.sh --check    # exit 1 if the committed ERD is stale (no write)
#
# No database is required: the generator reads the AppDbContext model in memory.
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repo_root"

dotnet run \
  --project backend/tools/UKPS.ErdGenerator \
  -- \
  --groups documentation/database/erd-groups.yml \
  --output documentation/database/ukps-erd.mmd \
  --html documentation/database/ukps-erd.html \
  "$@"
