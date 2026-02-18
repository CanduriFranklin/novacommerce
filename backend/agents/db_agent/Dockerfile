# syntax=docker/dockerfile:1.4

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the DbAgent project file and restore dependencies
COPY ["backend/agents/db_agent/src/DbAgent.csproj", "backend/agents/db_agent/src/"]
RUN dotnet restore "backend/agents/db_agent/src/DbAgent.csproj"

# Copy the remaining source code and build the project
COPY . .
WORKDIR /src/backend/agents/db_agent/src
RUN dotnet build "DbAgent.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/backend/agents/db_agent/src
RUN dotnet publish "DbAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0.100 AS final
WORKDIR /app

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables for runtime (no sensitive defaults)
ENV DOTNET_ENVIRONMENT="Production"
# These should be injected by Kubernetes/Secret Manager
# ENV SQL__CONNECTION_STRING=""

ENTRYPOINT ["dotnet", "DbAgent.dll"]
