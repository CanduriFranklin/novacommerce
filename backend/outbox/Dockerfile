# syntax=docker/dockerfile:1.4

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the Outbox project file and restore dependencies
COPY ["backend/outbox/src/Outbox.csproj", "backend/outbox/src/"]
COPY ["contracts/contracts.csproj", "contracts/"]
RUN dotnet restore "backend/outbox/src/Outbox.csproj"

# Copy the remaining source code and build the project
COPY . .
WORKDIR /src/backend/outbox/src
RUN dotnet build "Outbox.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/backend/outbox/src
RUN dotnet publish "Outbox.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0.100 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published application
COPY --from=publish /app/publish .

# Set environment variables for runtime (no sensitive defaults)
ENV ASPNETCORE_URLS="http://+:8080;https://+:8081"
ENV ASPNETCORE_ENVIRONMENT="Production"
# These should be injected by Kubernetes/Secret Manager
# ENV SQL__CONNECTION_STRING=""
# ENV RABBITMQ__HOST="rabbitmq"
# ENV RABBITMQ__USER=""
# ENV RABBITMQ__PASSWORD=""

ENTRYPOINT ["dotnet", "Outbox.dll"]
