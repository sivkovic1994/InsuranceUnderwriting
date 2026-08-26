# Insurance Underwriting Demo

A small demo project illustrating Domain-Driven Design, Event Sourcing and Clean Architecture,
built around an insurance underwriting process (risk assessment and policy issuance).

## Stack

- .NET 9 / ASP.NET Core Web API
- [Marten](https://martendb.io/) — event store and document store on PostgreSQL
- PostgreSQL 16 (local, via Docker)
- MediatR — CQRS-lite command dispatch to handlers
- xUnit — unit tests

## Architecture

Dependencies point inward only: `Api → Application → Domain`, while `Infrastructure`
implements the interfaces defined in the `Application` layer.

```
src/
├── InsuranceUnderwriting.Domain          # Aggregate, events, domain services — no technical dependencies
├── InsuranceUnderwriting.Application     # Commands, handlers (MediatR), repository interface
├── InsuranceUnderwriting.Infrastructure  # Marten repository implementation
└── InsuranceUnderwriting.Api             # Controllers, DI, configuration

tests/
└── InsuranceUnderwriting.Tests           # Unit tests for domain services and the aggregate
```

## Domain flow

1. A client submits an insurance application (`ApplicationSubmitted`)
2. The system assesses risk (`RiskAssessed`)
3. The premium is calculated based on the assessed risk (`PremiumCalculated`)
4. The policy is approved (`PolicyApproved`)

Each step appends an event to the Marten event store (PostgreSQL), while the current state
of the `InsuranceApplication` aggregate is derived via an inline snapshot projection.

## Running locally

```bash
docker compose up -d
dotnet run --project src/InsuranceUnderwriting.Api
```

Swagger UI is available at `/swagger` in the development environment.

## API endpoints

| Method | Route                                       | Description                  |
|--------|----------------------------------------------|-------------------------------|
| POST   | `/api/applications`                          | Submit a new application      |
| POST   | `/api/applications/{id}/assess-risk`         | Assess risk                   |
| POST   | `/api/applications/{id}/calculate-premium`   | Calculate premium             |
| POST   | `/api/applications/{id}/approve`             | Approve the policy            |
| GET    | `/api/applications/{id}`                     | Get current application state |

## Tests

```bash
dotnet test
```
