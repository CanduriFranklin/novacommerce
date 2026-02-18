# Use the .NET 10 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0.100 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY backend/payments/src/Payments.csproj .
RUN dotnet restore

# Copy the rest of the application code
COPY backend/payments/src/. .

# Build the application
RUN dotnet build "Payments.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "Payments.csproj" -c Release -o /app/publish

# Use the .NET 10 ASP.NET runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:10.0.100 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Payments.dll"]
