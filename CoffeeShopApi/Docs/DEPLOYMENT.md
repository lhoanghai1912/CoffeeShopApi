# Deployment Guide

## 🚀 Overview

Hướng dẫn triển khai CoffeeShopApi lên các môi trường khác nhau.

---

## 📋 Table of Contents

1. [Prerequisites](#prerequisites)
2. [Local Development Setup](#local-development-setup)
3. [Database Migration](#database-migration)
4. [Configuration](#configuration)
5. [IIS Deployment](#iis-deployment)
6. [Docker Deployment](#docker-deployment)
7. [Azure Deployment](#azure-deployment)
8. [CI/CD with GitHub Actions](#cicd-with-github-actions)
9. [Monitoring & Logging](#monitoring--logging)
10. [Troubleshooting](#troubleshooting)

---

## 🔧 Prerequisites

### Required Software

| Software | Version | Purpose |
|----------|---------|---------|
| .NET SDK | 8.0 | Runtime |
| SQL Server | 2019+ | Database |
| Visual Studio | 2022+ | IDE (optional) |
| Git | Latest | Version control |
| Node.js | 18+ | Frontend (if needed) |

### System Requirements

**Development:**
- CPU: 2+ cores
- RAM: 8GB+
- Disk: 10GB+ free space

**Production:**
- CPU: 4+ cores
- RAM: 16GB+
- Disk: 50GB+ SSD

---

## 💻 Local Development Setup

### 1. Clone Repository

```bash
git clone https://github.com/lhoanghai1912/CoffeeShopApi.git
cd CoffeeShopApi
```

### 2. Install Dependencies

```bash
cd CoffeeShopApi
dotnet restore
```

### 3. Update Connection String

Edit `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoffeeShopDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "http://localhost:5000",
    "Audience": "http://localhost:5000"
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@gmail.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "noreply@coffeeshop.com",
    "FromName": "CoffeeShop"
  }
}
```

### 4. Run Migrations

```bash
# Apply migrations
dotnet ef database update

# Or create new migration
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 5. Seed Data (Optional)

Uncomment in `Program.cs`:

```csharp
// Seed database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.SeedAsync(context);
}
```

### 6. Run Application

```bash
# Development mode with hot reload
dotnet watch run

# Or standard run
dotnet run
```

### 7. Verify Setup

Open browser:
- **Swagger UI:** https://localhost:5001/swagger
- **API:** https://localhost:5001/api/products

---

## 🗄️ Database Migration

### EF Core Commands

```bash
# Install EF Core tools (if not installed)
dotnet tool install --global dotnet-ef

# Update to latest version
dotnet tool update --global dotnet-ef

# Create new migration
dotnet ef migrations add <MigrationName>

# Apply migrations
dotnet ef database update

# Rollback to specific migration
dotnet ef database update <MigrationName>

# Remove last migration (if not applied)
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script

# Drop database
dotnet ef database drop
```

### Migration Best Practices

```csharp
// ✅ Good: Descriptive name
dotnet ef migrations add AddVoucherTables

// ❌ Bad: Generic name
dotnet ef migrations add UpdateDatabase

// Always review generated migration before applying
```

### Production Migration

```bash
# Generate SQL script for production
dotnet ef migrations script --output migrations.sql --idempotent

# Apply script manually in production
sqlcmd -S server -d CoffeeShopDb -i migrations.sql
```

---

## ⚙️ Configuration

### Environment-Specific Settings

**appsettings.json** (Default):
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=CoffeeShopDb_Dev;..."
  }
}
```

**appsettings.Production.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Error"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-server;Database=CoffeeShopDb;..."
  },
  "Jwt": {
    "Key": "production-secret-key-from-env",
    "Issuer": "https://api.coffeeshop.com",
    "Audience": "https://app.coffeeshop.com"
  }
}
```

### Environment Variables

```bash
# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT="Production"
$env:ConnectionStrings__DefaultConnection="Server=...;Database=...;"

# Linux/Mac
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Server=...;Database=...;"

# Docker
-e ASPNETCORE_ENVIRONMENT=Production
-e ConnectionStrings__DefaultConnection="..."
```

### Secrets Management

**Development (User Secrets):**

```bash
# Initialize user secrets
dotnet user-secrets init

# Set secret
dotnet user-secrets set "Jwt:Key" "your-secret-key"
dotnet user-secrets set "EmailSettings:SmtpPassword" "your-password"

# List secrets
dotnet user-secrets list

# Remove secret
dotnet user-secrets remove "Jwt:Key"

# Clear all secrets
dotnet user-secrets clear
```

**Production (Azure Key Vault):**

```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

---

## 🖥️ IIS Deployment

### 1. Publish Application

```bash
# Publish for IIS
dotnet publish -c Release -o ./publish

# Or with specific runtime
dotnet publish -c Release -r win-x64 --self-contained false -o ./publish
```

### 2. Install Prerequisites on Server

- **.NET 8 Hosting Bundle:** https://dotnet.microsoft.com/download/dotnet/8.0
- **IIS with ASP.NET Core Module**

```powershell
# Check if installed
dotnet --list-runtimes

# Should see:
# Microsoft.AspNetCore.App 8.0.x
```

### 3. Create IIS Site

**PowerShell (Admin):**

```powershell
# Import IIS module
Import-Module WebAdministration

# Create Application Pool
New-WebAppPool -Name "CoffeeShopApiPool"
Set-ItemProperty IIS:\AppPools\CoffeeShopApiPool -Name managedRuntimeVersion -Value ""
Set-ItemProperty IIS:\AppPools\CoffeeShopApiPool -Name processModel.identityType -Value NetworkService

# Create Website
New-Website -Name "CoffeeShopApi" -Port 80 -PhysicalPath "C:\inetpub\wwwroot\CoffeeShopApi" -ApplicationPool "CoffeeShopApiPool"

# Set permissions
icacls "C:\inetpub\wwwroot\CoffeeShopApi" /grant "IIS AppPool\CoffeeShopApiPool:(OI)(CI)(RX)"
icacls "C:\inetpub\wwwroot\CoffeeShopApi\wwwroot" /grant "IIS AppPool\CoffeeShopApiPool:(OI)(CI)(M)"
```

**IIS Manager GUI:**

1. Open IIS Manager
2. Right-click **Sites** → **Add Website**
3. Site name: `CoffeeShopApi`
4. Physical path: `C:\inetpub\wwwroot\CoffeeShopApi`
5. Binding: `http`, Port `80`, Host name: `api.coffeeshop.com`
6. Application Pool: Select `CoffeeShopApiPool` (No Managed Code)

### 4. Configure web.config

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\CoffeeShopApi.dll" 
                  stdoutLogEnabled="true" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess">
        <environmentVariables>
          <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>
```

### 5. Update appsettings.Production.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=prod-sql-server;Database=CoffeeShopDb;User Id=sa;Password=YourStrongPassword;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "production-secret-key-from-env-var",
    "Issuer": "https://api.coffeeshop.com",
    "Audience": "https://app.coffeeshop.com"
  }
}
```

### 6. Restart IIS

```powershell
iisreset

# Or restart specific app pool
Restart-WebAppPool -Name "CoffeeShopApiPool"
```

### 7. Verify Deployment

- Check logs: `C:\inetpub\wwwroot\CoffeeShopApi\logs\`
- Test API: `http://api.coffeeshop.com/swagger`

---

## 🐳 Docker Deployment

### 1. Create Dockerfile

**CoffeeShopApi/Dockerfile:**

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["CoffeeShopApi/CoffeeShopApi.csproj", "CoffeeShopApi/"]
RUN dotnet restore "CoffeeShopApi/CoffeeShopApi.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/CoffeeShopApi"
RUN dotnet build "CoffeeShopApi.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "CoffeeShopApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CoffeeShopApi.dll"]
```

### 2. Create docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    build:
      context: .
      dockerfile: CoffeeShopApi/Dockerfile
    ports:
      - "5000:80"
      - "5001:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=CoffeeShopDb;User Id=sa;Password=YourStrongPassword123!;TrustServerCertificate=True;
      - Jwt__Key=your-super-secret-key-min-32-characters-long
      - Jwt__Issuer=http://localhost:5000
      - Jwt__Audience=http://localhost:5000
    depends_on:
      - sqlserver
    networks:
      - coffeeshop-network

  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrongPassword123!
      - MSSQL_PID=Express
    ports:
      - "1433:1433"
    volumes:
      - sqlserver-data:/var/opt/mssql
    networks:
      - coffeeshop-network

volumes:
  sqlserver-data:

networks:
  coffeeshop-network:
    driver: bridge
```

### 3. Build & Run

```bash
# Build images
docker-compose build

# Start containers
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop containers
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

### 4. Apply Migrations in Docker

```bash
# Exec into API container
docker exec -it coffeeshopapi_api_1 /bin/bash

# Inside container
dotnet ef database update

# Or run migration from host
docker-compose exec api dotnet ef database update
```

### 5. Docker Commands

```bash
# List running containers
docker ps

# View logs
docker logs coffeeshopapi_api_1 -f

# Restart container
docker restart coffeeshopapi_api_1

# Remove container
docker rm -f coffeeshopapi_api_1

# Remove image
docker rmi coffeeshopapi_api

# Prune unused
docker system prune -a
```

---

## ☁️ Azure Deployment

### Option 1: Azure App Service

**1. Create App Service:**

```bash
# Login to Azure
az login

# Create resource group
az group create --name rg-coffeeshop --location eastus

# Create App Service Plan
az appservice plan create --name plan-coffeeshop --resource-group rg-coffeeshop --sku B1 --is-linux

# Create Web App
az webapp create --name api-coffeeshop --resource-group rg-coffeeshop --plan plan-coffeeshop --runtime "DOTNETCORE:8.0"
```

**2. Configure Connection String:**

```bash
az webapp config connection-string set --name api-coffeeshop --resource-group rg-coffeeshop --connection-string-type SQLAzure --settings DefaultConnection="Server=tcp:..."
```

**3. Deploy:**

```bash
# Publish
dotnet publish -c Release -o ./publish

# Zip files
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip

# Deploy
az webapp deploy --resource-group rg-coffeeshop --name api-coffeeshop --src-path ./publish.zip
```

### Option 2: Azure Container Instances

```bash
# Build and push image to Azure Container Registry
az acr create --name coffeeShopRegistry --resource-group rg-coffeeshop --sku Basic
az acr login --name coffeeShopRegistry
docker tag coffeeshopapi:latest coffeeshopregistry.azurecr.io/coffeeshopapi:latest
docker push coffeeshopregistry.azurecr.io/coffeeshopapi:latest

# Create container instance
az container create --resource-group rg-coffeeshop --name api-coffeeshop --image coffeeshopregistry.azurecr.io/coffeeshopapi:latest --cpu 1 --memory 1 --registry-login-server coffeeshopregistry.azurecr.io --registry-username <username> --registry-password <password> --ip-address Public --ports 80
```

---

## 🔄 CI/CD with GitHub Actions

### .github/workflows/deploy.yml

```yaml
name: Deploy to Production

on:
  push:
    branches: [ master ]
  workflow_dispatch:

env:
  AZURE_WEBAPP_NAME: api-coffeeshop
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - name: Checkout code
      uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal
    
    - name: Publish
      run: dotnet publish -c Release -o ./publish
    
    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v2
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### Setup Secrets

1. Go to GitHub repository → **Settings** → **Secrets and variables** → **Actions**
2. Add secrets:
   - `AZURE_WEBAPP_PUBLISH_PROFILE` (download from Azure Portal)
   - `CONNECTION_STRING`
   - `JWT_SECRET_KEY`

---

## 📊 Monitoring & Logging

### Serilog Configuration

**Install Packages:**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.Console
```

**Program.cs:**

```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Application Insights (Azure)

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
// appsettings.Production.json
{
  "ApplicationInsights": {
    "InstrumentationKey": "your-instrumentation-key"
  }
}

// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

---

## 🐛 Troubleshooting

### Common Issues

**1. 500 Internal Server Error**
```bash
# Check logs
cat logs/log-20250128.txt

# Enable detailed errors (dev only)
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
```

**2. Database Connection Failed**
```bash
# Test connection string
sqlcmd -S server -U sa -P password -Q "SELECT 1"

# Check firewall rules
```

**3. JWT Token Invalid**
```bash
# Verify JWT settings match between client & server
# Check token expiry
# Ensure Key is at least 32 characters
```

**4. CORS Issues**
```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

app.UseCors("AllowAll");
```

---

## 📖 Related Documentation

- 🏗️ [Architecture](./ARCHITECTURE.md)
- 🗄️ [Database Schema](./DATABASE.md)
- 📚 [README](./README.md)
