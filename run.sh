#!/usr/bin/env bash
set -e
cd "$(dirname "$0")"

if [ ! -d ".venv" ]; then
  python3 -m venv .venv
  .venv/bin/pip install --upgrade pip wheel setuptools
  .venv/bin/pip install -r requirements.txt
fi

if [ ! -f data/cert.pem ]; then
  .venv/bin/python scripts/make_cert.py
fi

HOST="${HOST:-0.0.0.0}"
PORT="${PORT:-8443}"

exec .venv/bin/python -m uvicorn backend.main:app \
  --host "$HOST" --port "$PORT" \
  --ssl-keyfile data/key.pem --ssl-certfile data/cert.pem
