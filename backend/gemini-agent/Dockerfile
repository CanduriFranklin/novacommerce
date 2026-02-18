# syntax=docker/dockerfile:1.4

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the GeminiAgent project file and restore dependencies
COPY ["backend/gemini-agent/src/GeminiAgent.csproj", "backend/gemini-agent/src/"]
COPY ["contracts/contracts.csproj", "contracts/"]
RUN dotnet restore "backend/gemini-agent/src/GeminiAgent.csproj"

# Copy the remaining source code and build the project
COPY . .
WORKDIR /src/backend/gemini-agent/src
RUN dotnet build "GeminiAgent.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/backend/gemini-agent/src
RUN dotnet publish "GeminiAgent.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
# ENV GEMINI__API_KEY=""

ENTRYPOINT ["dotnet", "GeminiAgent.dll"]
