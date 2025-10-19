Services:

- monolith (StudentDiary.Presentation) — http://localhost:5066
- activities-service — http://localhost:5286
- payment-service — http://localhost:5063

Run locally:

```bash
docker compose up --build -d
docker compose logs -f activities-service
```
