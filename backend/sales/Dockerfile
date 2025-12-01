# Use the official .NET 8 SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies first to leverage Docker layer caching
COPY ["services/sales/Sales.csproj", "services/sales/"]
RUN dotnet restore "services/sales/Sales.csproj"

# Copy the rest of the application code
COPY . .
WORKDIR "/src/services/sales"
RUN dotnet build "Sales.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Sales.csproj" -c Release -o /app/publish

# Use the official ASP.NET 8 runtime image for the final application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sales.dll"]
