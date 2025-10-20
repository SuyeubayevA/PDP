# Student Diary — Monorepo

> Small mono-repo containing services used in the StudentDiary sample:
>
> - `monolith` (Admin portal / Presentation layer)
> - `activities-service` (extra-curricular activities microservice)
> - `payment-service` (payments / billing)
> - `contracts` (shared DTOs / events used across services)

---

## Quick links (local dev)

- **Monolith (StudentDiary.Presentation)** — `http://localhost:5066`  
  Swagger: `http://localhost:5066/swagger`

- **Activities service** — `http://localhost:5286`  
  Swagger: `http://localhost:5286/swagger`

- **Payment service** — `http://localhost:5063`  
  Swagger: `http://localhost:5063/swagger`

> RabbitMQ management UI (if running via docker-compose):  
> `http://localhost:15672` (default) — if you remapped ports in `docker-compose.yml` check the host side port (e.g. `15673`).

---

## Services & endpoints

> **Note:** the list below is a compact reference — use each service Swagger UI for full contract details and example payloads.

### 1. `monolith` — StudentDiary.Presentation

**Purpose:** admin / presentation layer for the system. Exposes student management endpoints and acts as an orchestrator / UI API. Uses MediatR and the shared `contracts`.

**Typical endpoints**

- `GET /api/students` — list students
- `GET /api/students/{id}` — get student by id
- `POST /api/students` — create a new student (body: JSON `{ "name": "...", ... }`)
- `PUT /api/students/{id}` — update student

**Example (create student)**

```bash
curl -X POST http://localhost:5066/api/students \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Alice","lastName":"Smith","birthDate":"2016-04-20"}'
```

**Notes:** this service also contacts payment-service (via configured base URL) and publishes domain events via Outbox.

### 2. activities-service — ActivitiesService.Api

**Purpose:** manages activities (extracurricular classes), enrollments and related events. Consumes/produces domain events (MassTransit / RabbitMQ).

**Typical endpoints**

- `POST /api/activities` — create an activity
  Body example: { "name":"Chess club","capacity":20 }

- `POST /api/activities/{activityId}/enroll` — enroll a student (body: { "studentId": "..." })

- `GET /api/activities/{activityId}/enrollments` — list enrollments for an activity

**Examples**

#### Create activity

- `curl -X POST http://localhost:5286/api/activities \`
  `-H "Content-Type: application/json" \`
  `-d '{"name":"Robotics","capacity":12}'`

#### Enroll student

- `curl -X POST http://localhost:5286/api/activities/123/enroll \`
  `-H "Content-Type: application/json" \`
  `-d '{"studentId":"<student-guid>"}'`

**Messaging / reliability**

Consumers use MassTransit + RabbitMQ. The runtime config includes retry & (recommended) delayed redelivery → messages that repeatedly fail are moved to the {queue}\_error queue (DLQ) for inspection and replay.

This service uses an InMemory EF provider in dev by default (data is not persisted across restarts).

### 3. payment-service — PaymentService.Api

**Purpose:** payment processing / gateway façade used by the monolith and other services.

**Typical endpoints**

- `POST /api/payments — create a payment / charge (body: { "studentId":"...", "amount": 100.00, "currency":"EUR" })`

- `GET /api/payments/{id} — get payment status/details`

**Example**

- `curl -X POST http://localhost:5063/api/payments \`
  `-H "Content-Type: application/json" \`
  `-d '{"studentId":"<id>","amount":50.0,"currency":"EUR"}'`

### 4. contracts — School.Contracts

**Purpose:** shared DTOs/events used across services (e.g., ActivityCreatedEvent, StudentEnrolledEvent, request/response DTOs). Keep ProjectReference or package consistent across services.

Run locally (recommended)

From repository root:

# build images and run all services + infra (RabbitMQ, SQL if enabled)

- `docker compose up --build -d`

# follow service logs (example)

- `docker compose logs -f activities-service`

**Stop and remove:**

- `docker compose down`

**Force-rebuild when you changed code**
If you rebuilt your project locally but don't see changes in containers, force rebuild images with no cache:

- `docker compose build --no-cache`
- `docker compose up -d --force-recreate --build`

# Dev notes & troubleshooting

**InMemory DB** — by default services use EF UseInMemoryDatabase. This works for quick dev, but data is not persisted across restarts. For persistent dev DB use SQLite or SQL Server container and change USE_INMEMORY=false in env.

**Logs** — Serilog is configured. When running in containers logs are written to /app/logs/app.log. If you mount host volumes, logs will appear in ./logs/<service>/app.log.

**RabbitMQ** — When running via docker-compose, containers talk to RabbitMQ using service name rabbitmq:5672. If you need to connect from host tools, map the host ports (e.g. 5672:5672 or your custom mapping) and use amqp://guest:guest@localhost:<host-port>/.

**DLQ / poison messages** — consumers are configured to retry and then allow MassTransit to move failed messages to {queue}\_error. Inspect and replay from RabbitMQ Management UI when necessary.

**Hosted services** — OutboxProcessor is implemented as a background service.

### Reliability patterns, architecture notes (Outbox / CQRS / Clean Architecture)

**Outbox**

- Purpose: the Outbox pattern guarantees _atomicity_ between state changes and outgoing messages — you write both (business data + outbox record) in a single DB transaction, and a background worker (OutboxProcessor) later publishes the message to the broker. This prevents lost events when a publisher crashes after committing the DB but before sending the message.
- Where to look: `StudentDiary.Infrastructure.Outbox`, `OutboxRepository`, `OutboxProcessor`. The monolith/service that produces events writes to Outbox; the processor publishes to RabbitMQ (MassTransit).
- Notes / best practices:
  - Persist outbox records in the **same** transaction as domain updates.
  - Include metadata (MessageId, CorrelationId, AttemptCount) and keep messages idempotent.
  - Log publish failures and expose metrics (published / failed / retry counts).
  - Provide an admin replay path (re-publish outbox entries after a fix).

**CQRS (Command — Query Responsibility Segregation)**

- Purpose: separate _commands_ (mutations) and _queries_ (reads) to make intent explicit and simplify handlers. Commands are write-side use-cases and can produce domain events; queries are optimized for reads and return DTOs.
- Where to look: `*.Application` projects (MediatR commands/queries and handlers — e.g., `CreateStudentCommand`, `GetActivitiesQuery`). Controllers typically translate HTTP requests into Commands/Queries and send them through MediatR.
- Notes / best practices:
  - Keep command handlers focused on use-cases (no HTTP concerns there).
  - Queries should return lightweight DTOs or projections (avoid returning domain entities directly).
  - Use idempotency keys for commands that may be retried (payments, enrollments).
  - For complex read needs, consider a dedicated read model or materialized view.

**Clean Architecture + MVC (how it maps in this repo)**

- Short mapping:
  - **Presentation** = Controllers (API layer) — `StudentDiary.Presentation`, `ActivitiesService.Api`, `PaymentService.Api`. Controllers are _driving adapters_ that convert HTTP → Command/Query.
  - **Application** = Use Cases / Handlers (`*.Application`) — MediatR handlers, ports/interfaces to infrastructure.
  - **Domain** = Entities, ValueObjects, Domain rules (`*.Domain`). Pure business logic, no framework deps.
  - **Infrastructure** = Adapters (`*.Infrastructure`) — EF, MassTransit, HttpClients, Outbox implementation.
- MVC nuance: in Web API the **View** is JSON (no Razor). The “Model” that controllers use are **Request/Response DTOs** (presentation models) while the Domain Model stays in the domain layer. Keep mapping responsibilities explicit (use mappers/AutoMapper or mapping helpers in Application layer).
- Practical rules:
  - Controllers must be thin: validate, map to command/query, call MediatR, return response.
  - Application layer talks to infrastructure via interfaces (ports) — avoid `new`ing EF/MassTransit in handlers.
  - Keep domain layer free of framework (no EF attributes, no MassTransit types).
  - Put cross-cutting concerns (logging, retry policies) in Infrastructure or middleware.

**Quick checklist (practical)**

- [ ] Producers persist domain + outbox in one transaction.
- [ ] Background OutboxProcessor creates messages with `MessageId` and publishes via MassTransit.
- [ ] Consumers have retries + delayed redelivery; failures move to `{queue}_error` (DLQ).
- [ ] Commands are idempotent where necessary (payments/enrollments).
- [ ] Controllers map DTO ↔ Command/Query only; business logic stays in handlers/domain.
- [ ] Monitor outbox, DLQs and consumer retry/error queues (alerts).

# Monolit and Activities Service flow:

+------------+ +---------------+ +---------------------+ +-----------------------+
| Controller | --> | Command (HTTP)| --> | Domain + Outbox | --> | OutboxProcessor |
| (API) | | (map to Cmd) | | (same DB transaction)| | (background publishes)|
+------------+ +---------------+ +---------------------+ +-----------------------+
|
v
+-------------+
| RabbitMQ |
| exchange / |
| queue |
+-------------+
|
v
+-----------------+ +----------------+ |
| Consumer | ----> | Handler | |
| (MassTransit) | | (business) | |
+-----------------+ +----------------+ |
| |
| on repeated failures |
v |
+----------------------+ |
| Dead-Letter Queue | <------------------+
| ({queue}\_error) |
+----------------------+
