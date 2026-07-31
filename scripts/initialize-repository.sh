#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$repository_root"

if [[ ! -d .git ]]; then
  git init
fi

git branch -M main

if git remote get-url origin >/dev/null 2>&1; then
  git remote set-url origin https://github.com/RelFost/CShrimpSharp.git
else
  git remote add origin https://github.com/RelFost/CShrimpSharp.git
fi

git add .

if ! git diff --cached --quiet; then
  git commit -m 'feat: initialize CShrimpSharp'
fi

git push -u origin main
