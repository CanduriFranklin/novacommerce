# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY ["src/SupportAgent.csproj", "src/"]
RUN dotnet restore "src/SupportAgent.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src"
RUN dotnet build "SupportAgent.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "SupportAgent.csproj" -c Release -o /app/publish

# Stage 3: Final
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SupportAgent.dll"]
