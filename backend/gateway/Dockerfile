# syntax=docker/dockerfile:1.4

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the Gateway project file and restore dependencies
COPY ["backend/gateway/src/Gateway.csproj", "backend/gateway/src/"]
COPY ["contracts/contracts.csproj", "contracts/"]
RUN dotnet restore "backend/gateway/src/Gateway.csproj"

# Copy the remaining source code and build the project
COPY . .
WORKDIR /src/backend/gateway/src
RUN dotnet build "Gateway.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
WORKDIR /src/backend/gateway/src
RUN dotnet publish "Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

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
# ENV AUTH__JWT_SECRET=""
# ENV AUTH__ISSUER=""
# ENV AUTH__AUDIENCE=""
# ENV HEALTHCHECKS__IDENTITY__URI=""
# ENV HEALTHCHECKS__CATALOG__URI=""
# ENV OCELOT__GLOBALCONFIGURATION__BASEURL=""
# ENV OCELOT__ROUTES__0__UPSTREAMTEMPLATE=""
# ENV OCELOT__ROUTES__0__DOWNSTREAMTEMPLATELOGGING=""
# ENV OCELOT__ROUTES__0__DOWNSTREAMHOSTSANDPORTS__0__HOST=""
# ENV OCELOT__ROUTES__0__DOWNSTREAMHOSTSANDPORTS__0__PORT=""
# ENV OCELOT__ROUTES__0__UPSTREAMHTTPMETHOD__0="GET"
# ENV OCELOT__ROUTES__0__AUTHENTICATIONOPTIONS__AUTHENTICATIONSCHEME="Bearer"

ENTRYPOINT ["dotnet", "Gateway.dll"]
