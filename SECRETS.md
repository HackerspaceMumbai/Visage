# Secret Management Guide

This document explains how Visage handles sensitive information (API keys, connection strings, OAuth credentials) across different environments using .NET User Secrets for local development and Azure Key Vault with Managed Identity for production.

## Table of Contents
- [Local Development Setup](#local-development-setup)
- [Production Configuration (Azure Key Vault)](#production-configuration-azure-key-vault)
- [Secret Structure and Naming](#secret-structure-and-naming)
- [Security Best Practices](#security-best-practices)
- [Troubleshooting](#troubleshooting)

## Local Development Setup

### Prerequisites
- .NET 10 SDK installed
- Docker Desktop running (for local SQL Server)
- Repository cloned locally

### Step 1: Initialize User Secrets

All service projects and the AppHost already have User Secrets initialized with unique IDs:
- **AppHost**: `65b9ba5a-abb2-41d6-8ae4-bde74f1710d0`
- **Visage.FrontEnd.Web**: `37fcaacf-cfaf-4ad1-aece-f5ab7ac77bac`
- **Visage.Services.Eventing**: `fd364f8b-c50c-458d-8c30-ffc142f68a7e`
- **Visage.Services.Registrations**: `90004f07-39ee-43bc-ae8d-130d89aab454`

### Step 2: Configure Secrets

#### Quick Setup (Recommended)
Use the provided setup scripts to configure all secrets at once:

**Windows:**
```bash
./setup-oauth-secrets.bat
```

**Linux/Mac:**
```bash
chmod +x setup-oauth-secrets.sh
./setup-oauth-secrets.sh
```

#### Manual Configuration

Navigate to the AppHost directory and set all required parameters:

```bash
cd Visage.AppHost

# Auth0 Configuration
dotnet user-secrets set "Parameters:auth0-domain" "YOUR_AUTH0_DOMAIN.auth0.com"
dotnet user-secrets set "Parameters:auth0-clientid" "YOUR_AUTH0_CLIENT_ID"
dotnet user-secrets set "Parameters:auth0-clientsecret" "YOUR_AUTH0_CLIENT_SECRET"
dotnet user-secrets set "Parameters:auth0-audience" "YOUR_AUTH0_API_AUDIENCE"

# OAuth Social Providers
dotnet user-secrets set "Parameters:oauth-linkedin-clientid" "YOUR_LINKEDIN_CLIENT_ID"
dotnet user-secrets set "Parameters:oauth-linkedin-clientsecret" "YOUR_LINKEDIN_CLIENT_SECRET"
dotnet user-secrets set "Parameters:oauth-github-clientid" "YOUR_GITHUB_CLIENT_ID"
dotnet user-secrets set "Parameters:oauth-github-clientsecret" "YOUR_GITHUB_CLIENT_SECRET"

# Optional: OAuth base URL override (for proxy/production testing)
dotnet user-secrets set "Parameters:oauth-baseurl" "https://localhost:7400"

# Cloudinary Configuration
dotnet user-secrets set "Parameters:cloudinary-cloudname" "YOUR_CLOUDINARY_CLOUD_NAME"
dotnet user-secrets set "Parameters:cloudinary-apikey" "YOUR_CLOUDINARY_API_KEY"
dotnet user-secrets set "Parameters:cloudinary-apisecret" "YOUR_CLOUDINARY_API_SECRET"

# Clarity Analytics (optional)
dotnet user-secrets set "Parameters:clarity-projectid" "YOUR_CLARITY_PROJECT_ID"
```

### Step 3: Verify Configuration

Check that all secrets are properly set:

```bash
# In AppHost directory
dotnet user-secrets list

# You should see all Parameters prefixed with "Parameters:"
```

### Step 4: Run the Application

```bash
cd Visage.AppHost
dotnet run
# Or use: aspire run
```

The Aspire dashboard will open at `https://localhost:17044/` showing all services and their health status.

## Production Configuration (Azure Key Vault)

### Prerequisites
- Azure subscription
- Azure Key Vault created
- Managed Identity configured for the hosting environment

### Step 1: Create Azure Key Vault

```bash
# Using Azure CLI
az keyvault create \
  --name visage-keyvault \
  --resource-group visage-rg \
  --location eastus \
  --enable-rbac-authorization true
```

### Step 2: Configure Managed Identity RBAC

The application uses **Managed Identity** for passwordless authentication to Azure Key Vault. You need to assign the following RBAC role:

**Required Role**: `Key Vault Secrets User`

```bash
# Get the Managed Identity Object ID (for Azure Container Apps or App Service)
MANAGED_IDENTITY_ID=$(az identity show \
  --name visage-managed-identity \
  --resource-group visage-rg \
  --query principalId -o tsv)

# Assign Key Vault Secrets User role
az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $MANAGED_IDENTITY_ID \
  --scope /subscriptions/{subscription-id}/resourceGroups/visage-rg/providers/Microsoft.KeyVault/vaults/visage-keyvault
```

### Step 3: Add Secrets to Key Vault

Secret names in Azure Key Vault should follow the double-dash naming convention to match Aspire's parameter system:

```bash
# Auth0 Configuration
az keyvault secret set --vault-name visage-keyvault --name Parameters--auth0-domain --value "YOUR_AUTH0_DOMAIN.auth0.com"
az keyvault secret set --vault-name visage-keyvault --name Parameters--auth0-clientid --value "YOUR_AUTH0_CLIENT_ID"
az keyvault secret set --vault-name visage-keyvault --name Parameters--auth0-clientsecret --value "YOUR_AUTH0_CLIENT_SECRET"
az keyvault secret set --vault-name visage-keyvault --name Parameters--auth0-audience --value "YOUR_AUTH0_API_AUDIENCE"

# OAuth Social Providers
az keyvault secret set --vault-name visage-keyvault --name Parameters--oauth-linkedin-clientid --value "YOUR_LINKEDIN_CLIENT_ID"
az keyvault secret set --vault-name visage-keyvault --name Parameters--oauth-linkedin-clientsecret --value "YOUR_LINKEDIN_CLIENT_SECRET"
az keyvault secret set --vault-name visage-keyvault --name Parameters--oauth-github-clientid --value "YOUR_GITHUB_CLIENT_ID"
az keyvault secret set --vault-name visage-keyvault --name Parameters--oauth-github-clientsecret --value "YOUR_GITHUB_CLIENT_SECRET"

# Cloudinary Configuration
az keyvault secret set --vault-name visage-keyvault --name Parameters--cloudinary-cloudname --value "YOUR_CLOUDINARY_CLOUD_NAME"
az keyvault secret set --vault-name visage-keyvault --name Parameters--cloudinary-apikey --value "YOUR_CLOUDINARY_API_KEY"
az keyvault secret set --vault-name visage-keyvault --name Parameters--cloudinary-apisecret --value "YOUR_CLOUDINARY_API_SECRET"

# Clarity Analytics
az keyvault secret set --vault-name visage-keyvault --name Parameters--clarity-projectid --value "YOUR_CLARITY_PROJECT_ID"
```

### Step 4: Configure Application Settings

Set the Key Vault name in your production environment variables or appsettings.Production.json:

```json
{
  "KeyVault": {
    "VaultName": "visage-keyvault"
  }
}
```

**Important**: The vault name is the only configuration needed. The application uses `DefaultAzureCredential` which automatically discovers the Managed Identity.

### Step 5: Deploy with Aspire

When deploying to Azure Container Apps via Aspire:

```bash
# The Aspire deployment automatically:
# 1. Detects the Azure Key Vault configuration
# 2. Assigns Managed Identity to the container
# 3. Maps Key Vault secrets to environment variables
azd deploy
```

## Secret Structure and Naming

### Aspire Parameter Convention

Aspire uses a specific parameter naming pattern:
- **User Secrets**: `Parameters:{parameter-name}`
- **Key Vault**: `Parameters--{parameter-name}` (double-dash separator)
- **Environment Variables**: Automatically mapped by Aspire

### How Secrets Flow Through the System

1. **AppHost** defines parameters using `builder.AddParameter()`
2. **User Secrets** (Development) store values under `Parameters:` prefix
3. **Azure Key Vault** (Production) stores values under `Parameters--` prefix
4. **Service Projects** receive secrets as environment variables via `.WithEnvironment()`

Example:
```csharp
// In AppHost.cs
var iamClientSecret = builder.AddParameter("auth0-clientsecret", secret: true);

// Mapped to service
webapp.WithEnvironment("Auth0__ClientSecret", iamClientSecret)

// Service receives as: Auth0__ClientSecret environment variable
// Which binds to: builder.Configuration["Auth0:ClientSecret"]
```

### Required Secrets

| Parameter Name | Description | Required | Secret |
|---------------|-------------|----------|--------|
| `auth0-domain` | Auth0 tenant domain | Yes | No |
| `auth0-clientid` | Auth0 application client ID | Yes | No |
| `auth0-clientsecret` | Auth0 application client secret | Yes | Yes |
| `auth0-audience` | Auth0 API audience identifier | Yes | No |
| `oauth-linkedin-clientid` | LinkedIn OAuth app ID | Yes | Yes |
| `oauth-linkedin-clientsecret` | LinkedIn OAuth secret | Yes | Yes |
| `oauth-github-clientid` | GitHub OAuth app ID | Yes | Yes |
| `oauth-github-clientsecret` | GitHub OAuth secret | Yes | Yes |
| `oauth-baseurl` | Override OAuth redirect URL | No | No |
| `cloudinary-cloudname` | Cloudinary cloud name | Yes | No |
| `cloudinary-apikey` | Cloudinary API key | Yes | Yes |
| `cloudinary-apisecret` | Cloudinary API secret | Yes | Yes |
| `clarity-projectid` | Microsoft Clarity project ID | No | No |

## Security Best Practices

### ✅ DO
- Store all secrets in User Secrets during local development
- Use Azure Key Vault with Managed Identity in production
- Mark sensitive parameters with `secret: true` in AppHost.cs
- Rotate secrets regularly
- Use different secrets for each environment (dev, staging, prod)
- Review Aspire dashboard logs to ensure secrets are properly redacted

### ❌ DON'T
- Commit secrets to source control (`.gitignore` already excludes appsettings.Development.json)
- Share User Secrets files between developers
- Use production secrets in local development
- Store secrets in appsettings.json
- Use the same OAuth apps for dev and production

### Secret Redaction

Aspire automatically redacts parameters marked with `secret: true`:
- Dashboard logs show `[REDACTED]` instead of actual values
- Traces and metrics exclude secret values
- Environment variables are hidden in the dashboard

## Troubleshooting

### Problem: "KeyVault:VaultName configuration is required"

**Solution**: This error occurs in non-Development environments when the vault name isn't configured.

1. Check your environment: `ASPNETCORE_ENVIRONMENT`
2. Ensure `appsettings.Production.json` or environment variable includes:
   ```json
   {
     "KeyVault": {
       "VaultName": "your-vault-name"
     }
   }
   ```

### Problem: "User does not have secrets get permission"

**Solution**: The Managed Identity doesn't have proper RBAC permissions.

```bash
# Verify role assignment
az role assignment list \
  --assignee $MANAGED_IDENTITY_ID \
  --scope /subscriptions/{sub-id}/resourceGroups/visage-rg/providers/Microsoft.KeyVault/vaults/visage-keyvault

# If missing, assign the role (see Step 2 above)
```

### Problem: Secrets not appearing in services

**Solution**: Check the parameter mapping in `AppHost.cs`:

1. Verify parameter is defined: `builder.AddParameter("parameter-name", secret: true)`
2. Check it's passed to service: `.WithEnvironment("Config__Key", parameterRef)`
3. View in Aspire dashboard → Environment Variables tab

### Problem: OAuth redirects fail with "invalid redirect_uri"

**Solution**: 
1. Verify `oauth-baseurl` parameter matches your application URL
2. Check OAuth provider settings allow the callback URL
3. Review logs in `CloudinaryImageSigning` service

### Problem: "Secret not found in Key Vault"

**Solution**: Ensure secret names use double-dash separator:
- ❌ Wrong: `Parameters:auth0-clientsecret`
- ✅ Correct: `Parameters--auth0-clientsecret`

## Migration from Legacy Configuration

If you have existing secrets in appsettings files:

1. **Extract secrets** from appsettings.json/appsettings.Development.json
2. **Add to User Secrets** using commands in Step 2
3. **Remove from appsettings** files (keep structure, remove values)
4. **Verify** the application still runs locally
5. **Deploy to Azure** with Key Vault configuration

Example migration:
```json
// Before (appsettings.Development.json)
{
  "Auth0": {
    "ClientSecret": "YOUR_SECRET_HERE"  // ❌ Don't commit
  }
}

// After (User Secrets)
dotnet user-secrets set "Parameters:auth0-clientsecret" "YOUR_SECRET_HERE"

// appsettings.Development.json (cleaned)
{
  "Auth0": {
    // Secret managed via Aspire parameters
  }
}
```

## Additional Resources

- [.NET User Secrets Documentation](https://learn.microsoft.com/aspnet/core/security/app-secrets)
- [Azure Key Vault Documentation](https://learn.microsoft.com/azure/key-vault/)
- [Aspire Configuration Management](https://learn.microsoft.com/dotnet/aspire/fundamentals/configuration)
- [Managed Identity Overview](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [OAuth Setup Guide](./Visage.FrontEnd.Web/Configuration/oauth-setup.md)

## Support

For issues or questions:
1. Check [Troubleshooting](#troubleshooting) section above
2. Review Aspire dashboard logs for error details
3. Open an issue on GitHub with logs and environment details
