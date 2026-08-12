# ABC Retail - Azure App Service Deployment Guide

This guide documents the deployment process for the ABC Retail application to Azure App Service.

## Prerequisites

- Azure subscription with appropriate permissions
- Azure CLI installed (optional, for CLI deployment)
- Visual Studio 2022 or later (optional, for Visual Studio deployment)
- .NET 8.0 SDK installed
- Azure Storage Account created with Tables, Blobs, Queues, and Files enabled

## Azure Storage Account Setup

Before deploying the application, you need an Azure Storage Account:

1. **Create Storage Account**:
   - Go to Azure Portal → Create a resource → Storage account
   - Choose a unique name (e.g., `abcretailstorage`)
   - Select region closest to your App Service
   - Performance: Standard
   - Redundancy: Locally-redundant storage (LRS) for development, Geo-redundant (GRS) for production

2. **Get Connection String**:
   - Navigate to your Storage Account → Access keys
   - Copy the "Connection string" from key1 or key2

## Azure App Service Configuration

### Step 1: Create Azure App Service

1. Go to Azure Portal → Create a resource → Web App
2. Configure the following:
   - **Name**: `[student_number]` (this creates URL: `http://[student_number].azurewebsites.net`)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Windows (recommended) or Linux
   - **Region**: Same as your Storage Account
   - **App Service Plan**: Choose appropriate tier (F1 Free for testing, B1 or higher for production)

### Step 2: Configure Connection Strings in Azure Portal

The application requires Azure Storage connection strings to be configured. There are two approaches:

#### Option A: Using Application Settings (Recommended)

1. Navigate to your App Service → Configuration → Application settings
2. Add the following settings:

| Name | Value |
|------|-------|
| `AzureStorageSettings__ConnectionString` | `DefaultEndpointsProtocol=https;AccountName=...` |
| `AzureStorageSettings__TableConnectionString` | (optional, use main ConnectionString if same) |
| `AzureStorageSettings__BlobConnectionString` | (optional, use main ConnectionString if same) |
| `AzureStorageSettings__QueueConnectionString` | (optional, use main ConnectionString if same) |
| `AzureStorageSettings__FileConnectionString` | (optional, use main ConnectionString if same) |

> **Note**: Use double underscores (`__`) to represent nested configuration in environment variables.

#### Option B: Using Connection Strings Section

1. Navigate to your App Service → Configuration → Connection strings
2. Add a new connection string:
   - **Name**: `AzureStorage`
   - **Value**: Your Azure Storage connection string
   - **Type**: Custom

Then update `appsettings.Production.json` to read from this connection string.

### Step 3: Configure Health Check

1. Navigate to your App Service → Health check
2. Enable health check
3. Set path to: `/health`
4. Set probe interval (recommended: 30 seconds)

### Step 4: Enable Logging (Optional but Recommended)

1. Navigate to your App Service → App Service logs
2. Enable Application Logging (Filesystem)
3. Set Level to "Warning" or "Error"
4. Enable Web server logging

## Deployment Methods

### Method 1: Visual Studio Publish (Recommended for Development)

1. Right-click on the `ABCRetail` project in Solution Explorer
2. Select "Publish..."
3. Choose "Azure" → "Azure App Service (Windows)"
4. Sign in to your Azure account
5. Select your subscription and App Service
6. Click "Publish"

**Publish Profile Settings**:
- Configuration: Release
- Target Framework: net8.0
- Deployment Mode: Framework-Dependent
- Target Runtime: win-x64 (or linux-x64 for Linux App Service)

### Method 2: Azure CLI Deployment

```bash
# Login to Azure
az login

# Set your subscription (if you have multiple)
az account set --subscription "Your Subscription Name"

# Build the application
dotnet publish -c Release -o ./publish

# Deploy to Azure App Service
az webapp deploy --resource-group "YourResourceGroup" \
                 --name "[student_number]" \
                 --src-path ./publish \
                 --type zip

# Alternative: Deploy using zip deployment
cd ./publish
zip -r ../deploy.zip .
cd ..
az webapp deployment source config-zip \
    --resource-group "YourResourceGroup" \
    --name "[student_number]" \
    --src deploy.zip
```

### Method 3: GitHub Actions (CI/CD)

Create `.github/workflows/azure-deploy.yml`:

```yaml
name: Deploy to Azure App Service

on:
  push:
    branches:
      - main

env:
  AZURE_WEBAPP_NAME: '[student_number]'
  AZURE_WEBAPP_PACKAGE_PATH: './publish'
  DOTNET_VERSION: '8.0.x'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4

    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: ${{ env.DOTNET_VERSION }}

    - name: Restore dependencies
      run: dotnet restore

    - name: Build
      run: dotnet build --configuration Release --no-restore

    - name: Publish
      run: dotnet publish -c Release -o ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}

    - name: Deploy to Azure Web App
      uses: azure/webapps-deploy@v3
      with:
        app-name: ${{ env.AZURE_WEBAPP_NAME }}
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ${{ env.AZURE_WEBAPP_PACKAGE_PATH }}
```

**Setup GitHub Actions**:
1. Go to Azure Portal → Your App Service → Deployment Center → Manage publish profile
2. Download the publish profile
3. In GitHub, go to Settings → Secrets and variables → Actions
4. Create a new secret named `AZURE_WEBAPP_PUBLISH_PROFILE` with the publish profile content

### Method 4: FTP/FTPS Deployment

1. Go to Azure Portal → Your App Service → Deployment Center → FTPS credentials
2. Note the FTPS endpoint, username, and password
3. Use an FTP client (FileZilla, WinSCP) to upload published files
4. Build locally: `dotnet publish -c Release -o ./publish`
5. Upload contents of `./publish` folder to `/site/wwwroot/`

## Post-Deployment Verification

### Health Check Verification

After deployment, verify the application is running:

```bash
# Check health endpoint
curl https://[student_number].azurewebsites.net/health
# Should return: Healthy

# Or in browser, navigate to:
# https://[student_number].azurewebsites.net/health
```

### Functional Verification

1. Navigate to `https://[student_number].azurewebsites.net`
2. Verify the home page loads correctly
3. Test each feature:
   - Customer Profiles: Create, Read, Update, Delete
   - Products: Create, Read, Update, Delete
   - Images: Upload, View, Download, Delete
   - Multimedia: Upload, View, Download, Delete
   - Order Queue: Send message, View queue, Process message
   - Inventory Queue: Send message, View queue, Process message
   - Log Files: Create, View, Download, Delete

### Troubleshooting

**Application Won't Start**:
1. Check App Service Logs in Azure Portal → Log stream
2. Verify connection strings are configured correctly
3. Check if the Storage Account allows access from App Service IP

**Storage Connection Errors**:
1. Verify connection string format
2. Check Storage Account firewall settings
3. Ensure App Service has network access to Storage Account

**500 Internal Server Error**:
1. Enable detailed errors temporarily in web.config
2. Check Application Insights (if configured)
3. Review stdout logs in `/home/LogFiles/`

## Configuration Reference

### Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment | `Production` |
| `AzureStorageSettings__ConnectionString` | Main storage connection string | `DefaultEndpointsProtocol=https;...` |
| `AzureStorageSettings__CustomerTableName` | Customer table name | `customers` |
| `AzureStorageSettings__ProductTableName` | Product table name | `products` |
| `AzureStorageSettings__ImageContainerName` | Image blob container | `images` |
| `AzureStorageSettings__MultimediaContainerName` | Multimedia blob container | `multimedia` |
| `AzureStorageSettings__OrderQueueName` | Order queue name | `order-processing` |
| `AzureStorageSettings__InventoryQueueName` | Inventory queue name | `inventory-management` |
| `AzureStorageSettings__ImageNotificationQueueName` | Image notification queue | `image-notifications` |
| `AzureStorageSettings__LogFileShareName` | Log file share name | `logs` |

### App Service Configuration Checklist

- [ ] App Service created with correct runtime (.NET 8)
- [ ] Connection strings configured
- [ ] Health check enabled at `/health`
- [ ] HTTPS Only enabled (recommended)
- [ ] Application logging enabled
- [ ] Custom domain configured (if applicable)
- [ ] SSL certificate configured (if using custom domain)

## Security Considerations

1. **Connection Strings**: Never commit connection strings to source control. Use Azure App Service Configuration or Azure Key Vault.

2. **HTTPS**: Enable "HTTPS Only" in App Service Configuration → General settings.

3. **Storage Account Firewall**: Consider restricting Storage Account access to App Service IP addresses only.

4. **Managed Identity** (Advanced): Configure App Service Managed Identity for passwordless authentication to Azure Storage.

## URL Format

After successful deployment, the application will be accessible at:

- **HTTP**: `http://[student_number].azurewebsites.net`
- **HTTPS**: `https://[student_number].azurewebsites.net` (recommended)

Replace `[student_number]` with your actual Azure App Service name.
