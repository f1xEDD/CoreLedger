# Changelog

## Unreleased

## v0.2.0 - 2026-04-28

### Added
- Added transactional outbox support for reliable integration event delivery.
- Added `outbox_messages` table with message status, attempts, last error, and processed timestamp.
- Added `TransferCreatedEvent` integration event with stable event type `coreledger.transfer.created.v1`.
- Added outbox message creation during transfer creation in the same database transaction.
- Added `OutboxDispatcher` background service for processing pending outbox messages.
- Added `IEventPublisher` abstraction for publishing outbox events.
- Added `LoggingEventPublisher` for local/debug publishing.
- Added RabbitMQ local infrastructure via Docker Compose.
- Added `RabbitMqEventPublisher` for publishing outbox events to RabbitMQ.
- Added RabbitMQ configuration through `RabbitMqOptions`.
- Added RabbitMQ publisher integration test with Testcontainers.
- Added outbox dispatcher tests for successful publishing and retry/failure scenarios.

### Changed
- Outbox event publishing is now decoupled from transfer creation through `IEventPublisher`.
- `TransferService` now persists transfer data and integration events atomically.
- Local development configuration now includes RabbitMQ and outbox dispatcher settings.

### Docs
- Documented transactional outbox flow.
- Documented RabbitMQ local setup, management UI, exchange, and routing key.
- Updated development instructions for running PostgreSQL and RabbitMQ locally.

## v0.1.0 - 2026-04-28

### Added
- Added `POST /accounts`.
- Added `POST /transfers` with `Idempotency-Key` support.
- Added `GET /transfers/{id}`.
- Added `GET /accounts/{id}/balance`.
- Added Result-based error mapping.
- Added PostgreSQL integration tests with Testcontainers.
- Added health/readiness endpoints.
- Added OpenAPI/Scalar API reference for local development.

### Changed
- Introduced clean layering: Domain, Application, Infrastructure, Api.
- Switched expected application failures to `Result<T>` and `AppError`.

### Docs
- Added README with local run instructions and API examples.
