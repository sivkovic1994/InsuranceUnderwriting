# Insurance Underwriting Demo

A small demo project illustrating Domain-Driven Design, Event Sourcing, CQRS and a
microservices split, built around an insurance underwriting process (risk assessment
and policy issuance).

## Stack

- .NET 9 / ASP.NET Core Web API
- [Marten](https://martendb.io/) — event store (write side) and document store (read side) on PostgreSQL
- PostgreSQL 16 — one database per service
- Apache Kafka — asynchronous integration events between the two services
- Elasticsearch — full-text search index, fed by the same integration events as the Marten read model
- MediatR — command dispatch to handlers (write side)
- xUnit — unit tests

## Architecture

The application is split into two independent microservices communicating only through
Kafka — there is no synchronous call or shared database between them.

```
src/
├── Shared/
│   └── InsuranceUnderwriting.Contracts            # Integration events + Kafka topic name (the public contract)
│
├── Write/                                         # Command side — owns the event store
│   ├── InsuranceUnderwriting.Write.Domain         # Aggregate, domain events, domain services
│   ├── InsuranceUnderwriting.Write.Application    # Commands, handlers (MediatR), repository interface
│   ├── InsuranceUnderwriting.Write.Infrastructure # Marten event-store repository + Kafka producer
│   └── InsuranceUnderwriting.Write.Api            # Command controller (POST only), DI, configuration
│
└── Read/                                          # Query side — owns a denormalized read model
    ├── InsuranceUnderwriting.Read.Domain          # Denormalized view/search models
    ├── InsuranceUnderwriting.Read.Infrastructure  # Marten document store + Elasticsearch index + Kafka consumer that updates both
    └── InsuranceUnderwriting.Read.Api             # Query controller (GET only), DI, configuration

tests/
├── InsuranceUnderwriting.Write.Tests              # Domain services, aggregate, command handlers, event mapping
└── InsuranceUnderwriting.Read.Tests               # Integration-event contract (de)serialization
```

### Write service (event-sourced)

Every command loads/rehydrates the `InsuranceApplication` aggregate from Marten's event
store, appends a new domain event, and — once persisted — maps it to an **integration
event** and publishes it to the `insurance.application-events` Kafka topic (keyed by
`ApplicationId` so a given application's events stay in order within one partition).

### Read service (denormalized)

A background Kafka consumer subscribes to `insurance.application-events` and runs every
registered `IReadModelProjector` over each event, so a single message keeps two
independent stores in sync:

- **Marten** (`ApplicationReadModelUpdater`) — two documents, `ApplicationReadModel`
  (current state) and `ApplicationHistoryView` (audit trail), optimized for lookup by id.
  Marten here is used purely as a document store; there is no event sourcing on this side.
- **Elasticsearch** (`ElasticsearchProjector`) — one `ApplicationSearchDocument` per
  application in the `applications` index, optimized for full-text search across client
  name, insurance type and history entries — the kind of query a document-by-id store
  isn't built for.

Queries only ever read these two stores, never the write service's event store.

## Domain flow

1. A client submits an insurance application (`ApplicationSubmitted`)
2. The system assesses risk (`RiskAssessed`)
3. The premium is calculated based on the assessed risk (`PremiumCalculated`)
4. The policy is approved (`PolicyApproved`)

Each step appends an event to the Write service's event store and is asynchronously
propagated to the Read service via Kafka, so the read model is *eventually consistent*
with the write side.

## Running locally

```bash
docker compose up -d
dotnet run --project src/Write/InsuranceUnderwriting.Write.Api
dotnet run --project src/Read/InsuranceUnderwriting.Read.Api
```

`docker compose up -d` starts both PostgreSQL databases (`insurance_write`,
`insurance_read`), a single-node Kafka broker, and a single-node Elasticsearch instance
(security disabled — local/demo only). Swagger UI is available at `/swagger` on each
service in the development environment.

## API endpoints

**Write service** (commands only):

| Method | Route                                       | Description                  |
|--------|----------------------------------------------|-------------------------------|
| POST   | `/api/applications`                          | Submit a new application      |
| POST   | `/api/applications/{id}/assess-risk`         | Assess risk                   |
| POST   | `/api/applications/{id}/calculate-premium`   | Calculate premium             |
| POST   | `/api/applications/{id}/approve`             | Approve the policy            |

**Read service** (queries only):

| Method | Route                                       | Description                    |
|--------|----------------------------------------------|---------------------------------|
| GET    | `/api/applications/{id}`                     | Get current (denormalized) state |
| GET    | `/api/applications/{id}/history`             | Get the application's event history |
| GET    | `/api/applications/search?q={term}`          | Full-text search (client name, insurance type, history) via Elasticsearch |

## Tests

```bash
dotnet test
```
