# Solution and Project Structure (2026-02-18)

## Backend.sln (in /backend)
Includes all backend microservices, agents, and their test projects:
- agents/db_agent/src/DbAgent.csproj
- agents/inventory_agent/src/InventoryAgent.csproj
- agents/sales_agent/src/SalesAgent.csproj
- catalog/src/Catalog.csproj
- gateway/src/Gateway.csproj
- gemini-agent/src/GeminiAgent.csproj
- identity/src/Identity.csproj
- inventory/src/Inventory.csproj
- outbox/src/Outbox.csproj
- outbox/src/OutboxWorker.csproj
- payments/src/Payments.csproj
- sales/src/Sales.csproj
- users-auth/src/UsersAuth.csproj
- inventory/tests/unit/Inventory.UnitTests.csproj
- inventory/tests/integration/Inventory.IntegrationTests.csproj
- inventory/tests/performance/Inventory.PerformanceTests.csproj
- inventory/tests/contract/Inventory.ContractTests.csproj
- sales/tests/unit/Sales.UnitTests.csproj
- sales/tests/integration/Sales.IntegrationTests.csproj
- sales/tests/performance/Sales.PerformanceTests.csproj
- identity/tests/unit/Identity.UnitTests.csproj
- payments/tests/unit/Payments.UnitTests.csproj
- outbox/tests/performance/Outbox.PerformanceTests.csproj
- users-auth/tests/unit/UsersAuth.UnitTests.csproj
- users-auth/tests/integration/UsersAuth.IntegrationTests.csproj
- tests/e2e/E2E.Tests.csproj

## Webstore.sln (in /frontend/webstore)
Includes all frontend (webstore) projects:
- src/Webstore.csproj
- tests/unit/Webstore.UnitTests.csproj
- tests/integration/Webstore.IntegrationTests.csproj
- tests/performance/Webstore.PerformanceTests.csproj

## Excluded Folders
- infrastructure/ (Helm, k8s, terraform): Not C# code.
- contracts/: Not a C# project.
- docs/: Not code.
- Any Python code in agents/: Not included in .NET solutions.

## Rationale
- Only real C# projects are included in solutions.
- No C# code remains outside a .csproj.
- No duplicate or unnecessary projects.
- Structure is scalable and clear for enterprise/monorepo use.

