# Insurance Underwriting Demo

A small demo project illustrating Domain-Driven Design, Event Sourcing, CQRS and a
microservices split, built around an insurance underwriting process (risk assessment
and policy issuance).

## Stack

- .NET 9 / ASP.NET Core Web API
- [Marten](https://martendb.io/) — event store (write side) and document store (read side) on PostgreSQL
- PostgreSQL 16 — one database per service
- Apache Kafka — asynchronous integration events between the two services
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
    ├── InsuranceUnderwriting.Read.Domain          # Denormalized view models
    ├── InsuranceUnderwriting.Read.Infrastructure  # Marten document store + Kafka consumer that updates it
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

A background Kafka consumer subscribes to `insurance.application-events`, and applies
each integration event to two denormalized Marten documents — `ApplicationReadModel`
(current state) and `ApplicationHistoryView` (audit trail). Marten here is used purely
as a document store; there is no event sourcing on this side. Queries only ever read
these documents, never the write service's event store.

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
`insurance_read`) and a single-node Kafka broker. Swagger UI is available at `/swagger`
on each service in the development environment.

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

## Tests

```bash
dotnet test
```
