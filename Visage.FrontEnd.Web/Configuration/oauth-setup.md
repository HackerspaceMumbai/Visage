# OAuth Configuration Setup Guide

## Using .NET User Secrets (Recommended for Development)

### Quick Setup (Use the provided scripts):
```bash
# On Windows:
./setup-oauth-secrets.bat

# On Linux/Mac:
chmod +x setup-oauth-secrets.sh
./setup-oauth-secrets.sh
```

### Manual Setup:

#### 1. Initialize user secrets for the web project:
```bash
cd Visage.FrontEnd.Web
dotnet user-secrets init
```

#### 2. Set OAuth credentials using user secrets:
```bash
# LinkedIn OAuth credentials
dotnet user-secrets set "OAuth:LinkedIn:ClientId" "YOUR_LINKEDIN_CLIENT_ID"
dotnet user-secrets set "OAuth:LinkedIn:ClientSecret" "YOUR_LINKEDIN_CLIENT_SECRET"

# GitHub OAuth credentials  
dotnet user-secrets set "OAuth:GitHub:ClientId" "YOUR_GITHUB_CLIENT_ID"
dotnet user-secrets set "OAuth:GitHub:ClientSecret" "YOUR_GITHUB_CLIENT_SECRET"
```

#### 3. Set Aspire parameters (for AppHost):
```bash
# Navigate to AppHost project
cd ../Visage.AppHost

# Set OAuth parameters for Aspire
dotnet user-secrets set "Parameters:oauth-linkedin-clientid" "YOUR_LINKEDIN_CLIENT_ID"
dotnet user-secrets set "Parameters:oauth-linkedin-clientsecret" "YOUR_LINKEDIN_CLIENT_SECRET"
dotnet user-secrets set "Parameters:oauth-github-clientid" "YOUR_GITHUB_CLIENT_ID"
dotnet user-secrets set "Parameters:oauth-github-clientsecret" "YOUR_GITHUB_CLIENT_SECRET"
```

#### 4. Verify secrets are set:
```bash
# In Visage.FrontEnd.Web
dotnet user-secrets list

# In Visage.AppHost  
dotnet user-secrets list
```

## Production Configuration

### For Azure Container Apps (via Aspire):
Aspire automatically maps parameters to environment variables:
- `oauth-linkedin-clientid` → `OAuth__LinkedIn__ClientId`
- `oauth-linkedin-clientsecret` → `OAuth__LinkedIn__ClientSecret`
- `oauth-github-clientid` → `OAuth__GitHub__ClientId`
- `oauth-github-clientsecret` → `OAuth__GitHub__ClientSecret`

### For Azure Key Vault (Recommended for Production):
```csharp
// In Program.cs for production
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

## Configuration Structure
The OAuth configuration expects this structure:
```json
{
  "OAuth": {
    "LinkedIn": {
      "ClientId": "your_client_id",
      "ClientSecret": "your_client_secret"
    },
    "GitHub": {
      "ClientId": "your_client_id", 
      "ClientSecret": "your_client_secret"
    }
  }
}
```

## Aspire Best Practices
- Use `builder.AddParameter()` with `secret: true` for sensitive data
- Store secrets in user secrets for development
- Use environment variables in production
- Aspire automatically injects parameters as environment variables
- Never commit secrets to source control

## Security Notes
- **Never commit OAuth secrets to source control**
- User secrets are stored outside the project directory
- In production, use Azure Key Vault or secure environment variables
- Aspire automatically injects environment variables into containers
- Parameters marked with `secret: true` are redacted in logs