# syntax=docker/dockerfile:1.4

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the SalesAgent project file and restore dependencies
COPY ["backend/agents/sales_agent/src/SalesAgent.csproj", "backend/agents/sales_agent/src/"]
COPY ["contracts/contracts.csproj", "contracts/"]
RUN dotnet restore "backend/agents/sales_agent/src/SalesAgent.csproj"

# Copy the remaining source code and build the project
COPY . .
WORKDIR /src/backend/agents/sales_agent/src
RUN dotnet build "SalesAgent.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/backend/agents/sales_agent/src
RUN dotnet publish "SalesAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0.100 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables for runtime (no sensitive defaults)
ENV DOTNET_ENVIRONMENT="Production"
# These should be injected by Kubernetes/Secret Manager
# ENV RABBITMQ__HOST="rabbitmq"
# ENV RABBITMQ__USER=""
# ENV RABBITMQ__PASSWORD=""

ENTRYPOINT ["dotnet", "SalesAgent.dll"]
