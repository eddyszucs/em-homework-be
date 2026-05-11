#!/bin/bash
set -e

cd "$(dirname "$0")"

echo "==> Starting all services (PostgreSQL + Backend)..."
docker --context desktop-linux compose up -d

echo ""
echo "==> Waiting for backend to be healthy..."
for i in {1..30}; do
  if curl -sf http://localhost:5000/health > /dev/null 2>&1; then
    echo "==> Backend is up and healthy!"
    echo ""
    echo "==> Services running:"
    echo "   PostgreSQL: localhost:5432 (clinicdb)"
    echo "   Backend:   http://localhost:5000"
    echo "   Swagger:   http://localhost:5000/swagger"
    echo ""
    echo "==> Test login:"
    echo "   curl -X POST http://localhost:5000/api/auth/login \\"
    echo "     -H 'Content-Type: application/json' \\"
    echo "     -d '{\"username\":\"assistent1\",\"password\":\"Asst123!\"}'"
    echo ""
    echo "==> Docker setup verified successfully!"
    exit 0
  fi
  echo "  Waiting for backend... ($i/30)"
  sleep 2
done

echo "ERROR: Backend did not become healthy in time."
echo "Logs:"
docker --context desktop-linux compose logs backend
exit 1