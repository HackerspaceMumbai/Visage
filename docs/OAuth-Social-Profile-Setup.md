# OAuth Social Profile Authentication Setup Guide

## Overview

This document explains how to configure Auth0 social connections for LinkedIn and GitHub to enable OAuth-based profile verification in the Visage application. This prevents users from entering fake or incorrect profile URLs.

## Architecture

### Flow Diagram

```
User clicks "Connect LinkedIn/GitHub"
    ?
Frontend initiates Auth0 social connection
    ?
User redirects to LinkedIn/GitHub OAuth consent
    ?
User authorizes Visage to access profile
    ?
Auth0 exchanges code for access token
    ?
Auth0 redirects back to /registration/mandatory/oauth-callback
    ?
Frontend extracts profile URL from Auth0 claims
    ?
Frontend calls POST /api/profile/social/link-callback
    ?
Backend stores verified profile URL with IsVerified=true
    ?
User redirects back to registration form with verified badge
```

## Prerequisites

1. **Auth0 Account**: You must have an Auth0 account with a configured tenant
2. **LinkedIn Developer Account**: Required for LinkedIn OAuth
3. **GitHub OAuth App**: Required for GitHub OAuth

## Step 1: Configure LinkedIn Social Connection in Auth0

### 1.1 Create LinkedIn OAuth App

1. Go to [LinkedIn Developers](https://www.linkedin.com/developers/apps)
2. Click **Create app**
3. Fill in:
   - **App name**: Visage Event Management
   - **LinkedIn Page**: (Your organization page)
   - **Privacy policy URL**: https://yourdomain.com/privacy
   - **App logo**: Upload your logo
4. Click **Create app**
5. Navigate to **Auth** tab
6. Add redirect URLs:
   - Development: `https://YOUR_AUTH0_DOMAIN/login/callback`
   - Production: `https://YOUR_AUTH0_DOMAIN/login/callback`
7. Copy **Client ID** and **Client Secret**

### 1.2 Enable LinkedIn Connection in Auth0

1. Log in to [Auth0 Dashboard](https://manage.auth0.com/)
2. Navigate to **Authentication** ? **Social**
3. Click **+ Create Connection**
4. Select **LinkedIn**
5. Enter:
   - **Client ID**: (from Step 1.1)
   - **Client Secret**: (from Step 1.1)
   - **Attributes**:
     - ? Basic Profile
     - ? Email Address
   - **Permissions**: `r_emailaddress`, `r_liteprofile`
6. Click **Create**
7. Go to **Applications** tab
8. Enable the connection for your Visage application

## Step 2: Configure GitHub Social Connection in Auth0

### 2.1 Create GitHub OAuth App

1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click **New OAuth App**
3. Fill in:
   - **Application name**: Visage Event Management
   - **Homepage URL**: https://yourdomain.com
   - **Authorization callback URL**: `https://YOUR_AUTH0_DOMAIN/login/callback`
   - **Application description**: (Optional)
4. Click **Register application**
5. Copy **Client ID**
6. Click **Generate a new client secret**
7. Copy **Client Secret** (won't be shown again!)

### 2.2 Enable GitHub Connection in Auth0

1. Navigate to **Authentication** ? **Social**
2. Click **+ Create Connection**
3. Select **GitHub**
4. Enter:
   - **Client ID**: (from Step 2.1)
   - **Client Secret**: (from Step 2.1)
   - **Attributes**:
     - ? Basic Profile
     - ? Email Address
   - **Permissions**: `read:user`, `user:email`
5. Click **Create**
6. Go to **Applications** tab
7. Enable the connection for your Visage application

## Step 3: Configure Auth0 Scopes

### 3.1 Update Visage Auth0 Configuration

In `Visage.FrontEnd.Web/Program.cs`, ensure the following scopes are requested:

```csharp
builder.Services
    .AddAuth0WebAppAuthentication(options => {
        options.Domain = builder.Configuration["Auth0:Domain"];
        options.ClientId = builder.Configuration["Auth0:ClientId"];
        options.ClientSecret = builder.Configuration["Auth0:ClientSecret"];
        options.Scope = "openid profile email offline_access profile:read-write";
    })
```

### 3.2 Configure Custom Claims in Auth0 Rules

To extract social profile URLs, create an Auth0 Rule:

1. Navigate to **Auth Pipeline** ? **Rules**
2. Click **+ Create**
3. Name: `Add Social Profile URLs`
4. Paste the following code:

```javascript
function addSocialProfileUrls(user, context, callback) {
  const namespace = 'https://visage.hackmum.in';
  
  // Extract LinkedIn profile URL
  if (context.connection === 'linkedin') {
    const linkedInId = user.identities[0].userId;
    context.idToken[`${namespace}/linkedin_url`] = `https://www.linkedin.com/in/${linkedInId}`;
    context.accessToken[`${namespace}/linkedin_url`] = `https://www.linkedin.com/in/${linkedInId}`;
  }
  
  // Extract GitHub profile URL
  if (context.connection === 'github') {
    const githubUsername = user.nickname || user.identities[0].profileData.login;
    context.idToken[`${namespace}/github_url`] = `https://github.com/${githubUsername}`;
    context.accessToken[`${namespace}/github_url`] = `https://github.com/${githubUsername}`;
  }
  
  // Include connection name for callback processing
  context.idToken.connection = context.connection;
  context.accessToken.connection = context.connection;
  
  callback(null, user, context);
}
```

5. Click **Save**

## Step 4: Test Social Connections

### 4.1 Test LinkedIn Connection

1. Run Visage locally: `dotnet run --project Visage.AppHost`
2. Navigate to `/registration/mandatory`
3. Click **Connect LinkedIn**
4. You should be redirected to LinkedIn OAuth consent
5. Authorize the app
6. You should redirect back with a verified badge
7. Check database: `IsLinkedInVerified` should be `true`

### 4.2 Test GitHub Connection

1. Navigate to `/registration/mandatory`
2. Click **Connect GitHub**
3. You should be redirected to GitHub OAuth consent
4. Authorize the app
5. You should redirect back with a verified badge
6. Check database: `IsGitHubVerified` should be `true`

## Step 5: Migrate Existing Data (Optional)

If you have existing users with free-form LinkedIn/GitHub URLs, you can mark them as unverified:

```sql
-- Mark all existing social profiles as unverified
UPDATE Registrants 
SET IsLinkedInVerified = 0, 
    IsGitHubVerified = 0
WHERE LinkedInProfile IS NOT NULL 
   OR GitHubProfile IS NOT NULL;
```

Users will need to reconnect via OAuth to verify their profiles.

## Security Considerations

1. **No Manual Input**: Users cannot manually enter LinkedIn/GitHub URLs - they must authenticate via OAuth
2. **Verification Timestamp**: `LinkedInVerifiedAt` and `GitHubVerifiedAt` track when verification occurred
3. **Re-verification**: Users can disconnect and reconnect to update their profile URLs
4. **Scope Limitation**: Only request minimal scopes (`r_liteprofile`, `r_emailaddress` for LinkedIn; `read:user`, `user:email` for GitHub)

## Troubleshooting

### Error: "Invalid connection"

- **Cause**: Social connection not enabled in Auth0 application
- **Fix**: Go to Auth0 Dashboard ? Applications ? Your App ? Connections and enable LinkedIn/GitHub

### Error: "Could not determine social provider"

- **Cause**: Auth0 Rule not configured or connection claim missing
- **Fix**: Verify Auth0 Rule is active and includes `connection` in token

### Profile URL not extracted

- **Cause**: LinkedIn/GitHub profile data not available in claims
- **Fix**: Check Auth0 Rule logs and verify scopes are correctly requested

## API Reference

### POST /api/profile/social/link-callback

Stores OAuth-verified social profile URL.

**Request Body**:
```json
{
  "provider": "linkedin",
  "profileUrl": "https://www.linkedin.com/in/johndoe"
}
```

**Response**:
```json
{
  "message": "linkedin profile linked successfully",
  "profileUrl": "https://www.linkedin.com/in/johndoe",
  "verifiedAt": "2025-01-17T12:34:56Z"
}
```

### GET /api/profile/social/status

Retrieves social connection status.

**Response**:
```json
{
  "linkedIn": {
    "isConnected": true,
    "profileUrl": "https://www.linkedin.com/in/johndoe",
    "verifiedAt": "2025-01-17T12:34:56Z"
  },
  "gitHub": {
    "isConnected": false,
    "profileUrl": null,
    "verifiedAt": null
  }
}
```

## Related Files

- Backend API: `Visage.Services.Registrations/ProfileApi.cs`
- Frontend Service: `Visage.FrontEnd.Shared/Services/SocialAuthService.cs`
- OAuth Callback Page: `Visage.FrontEnd.Shared/Pages/OAuthCallback.razor`
- Registration Form: `Visage.FrontEnd.Shared/Components/MandatoryRegistration.razor`
- Data Models: `Visage.Shared/Models/SocialProfileLinkDto.cs`, `Visage.Shared/Models/Registrant.cs`
