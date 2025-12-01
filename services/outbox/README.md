# Outbox Worker Service

The Outbox Worker service is responsible for implementing the Outbox pattern to ensure reliable event delivery to RabbitMQ. It periodically reads pending messages from the `OutboxMessages` table, publishes them to RabbitMQ, and marks them as processed.

## Functional Requirements

- Periodically reads the `OutboxMessages` table in the database.
- Posts pending messages to RabbitMQ with delivery confirmation.
- Marks messages as processed in the Outbox table after successful posting.
- Handles retries in case of connection failure or posting error.
- Exposes basic metrics (e.g., messages processed, retries, failures) via OpenTelemetry.

## Technical Details

- **Project Type**: .NET 8.0 Worker Service.
- **Database Access**: Uses `Microsoft.EntityFrameworkCore` to interact with the `OutboxMessages` table.
- **Messaging**: Uses `RabbitMQ.Client` for publishing messages to RabbitMQ.
- **Logging**: Configured with `Serilog` for structured logging to console and file.
- **Observability**: Integrated with `OpenTelemetry` for tracing and metrics.
- **Configuration**: RabbitMQ connection string and SQL connection string are consumed via environment variables (`RABBITMQ_CONNECTION_STRING` and `SQL_CONNECTION_STRING`).

## Getting Started

### Prerequisites

- .NET SDK 8.0
- Docker (for running RabbitMQ and SQL Server locally, if not using existing instances)

### Environment Variables

The service requires the following environment variables to be set:

- `SQL_CONNECTION_STRING`: Connection string to the SQL Server database where the `OutboxMessages` table resides.
- `RABBITMQ_CONNECTION_STRING`: Connection string to the RabbitMQ instance (e.g., `amqp://guest:guest@localhost:5672`).

### Database Migrations

Before running the service, ensure the `OutboxMessages` table is created in your database. You can apply the migrations using Entity Framework Core CLI tools:

```bash
cd services/outbox_worker/src
dotnet ef database update --project OutboxWorker.csproj
```

### Running the Worker

1.  **Set Environment Variables**:
    ```bash
    export SQL_CONNECTION_STRING="Your_SQL_Connection_String"
    export RABBITMQ_CONNECTION_STRING="Your_RabbitMQ_Connection_String"
    ```
    (On Windows, use `set` instead of `export`)

2.  **Navigate to the project directory**:
    ```bash
    cd services/outbox_worker/src
    ```

3.  **Run the application**:
    ```bash
    dotnet run --project OutboxWorker.csproj
    ```

The worker will start polling the database for new messages and publishing them to RabbitMQ.

## Testing

### Unit Tests

To run unit tests:

```bash
cd services/outbox_worker/tests/Outbox.UnitTests
dotnet test
```

### Integration Tests

To run integration tests:

```bash
cd services/outbox_worker/tests/Outbox.IntegrationTests
dotnet test
```

## Project Structure

-   `src/OutboxWorker.csproj`: Main worker project.
-   `src/Services/OutboxProcessor.cs`: Contains the core logic for reading and publishing messages.
-   `src/Infrastructure/OutboxDbContext.cs`: Entity Framework Core DbContext for `OutboxMessages`.
-   `src/Infrastructure/OutboxMessage.cs`: Entity model for outbox messages.
-   `src/Infrastructure/RabbitMqPublisher.cs`: Handles publishing messages to RabbitMQ.
-   `src/Infrastructure/Migrations/`: Contains EF Core database migration files.
-   `tests/Outbox.UnitTests/`: Project for unit tests using `xunit` and `Moq`.
-   `tests/Outbox.IntegrationTests/`: Project for integration tests using `Microsoft.AspNetCore.Mvc.Testing`.
