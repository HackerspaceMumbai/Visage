# Direct OAuth Profile Verification System

## Overview
This system implements direct OAuth flows with LinkedIn and GitHub APIs to verify that users actually own their social media profiles. This is crucial for screening registrants and preventing fake profiles.

## Architecture Changes

### Key Differences from Auth0 Social Connections:
1. **Direct API Integration**: We bypass Auth0 and connect directly to LinkedIn and GitHub APIs
2. **Profile Ownership Verification**: Users must authenticate with the actual social platform
3. **Mandatory by Role**: 
   - **Working Professionals**: LinkedIn verification is mandatory, GitHub is optional
   - **Students**: GitHub verification is mandatory, LinkedIn is optional
4. **Verified Timestamps**: We track when each profile was verified

### Flow Diagram:
```
User clicks "Connect LinkedIn" 
    ?
App redirects to /oauth/linkedin/authorize
    ?  
Server generates state token and redirects to LinkedIn OAuth
    ?
User authenticates with LinkedIn
    ?
LinkedIn redirects to /oauth/linkedin/callback with code
    ?
Server exchanges code for access token
    ?
Server fetches user profile from LinkedIn API
    ?
Server redirects back to registration with verified profile URL
    ?
Frontend stores verified profile in database
```

## Setup Instructions

### 1. Create OAuth Applications

#### LinkedIn OAuth App:
1. Go to [LinkedIn Developer Console](https://www.linkedin.com/developers/apps)
2. Click "Create app"
3. Fill in:
   - **App name**: "Visage Event Management"
   - **LinkedIn Page**: Your organization page
   - **Privacy policy URL**: https://yourdomain.com/privacy
   - **App logo**: Upload your logo
4. In the "Auth" tab:
   - Add redirect URL: `https://yourdomain.com/oauth/linkedin/callback`
   - For development: `https://localhost:7400/oauth/linkedin/callback`
5. Copy **Client ID** and **Client Secret**
6. Request access to "Sign In with LinkedIn" product

#### GitHub OAuth App:
1. Go to [GitHub Developer Settings](https://github.com/settings/developers)
2. Click "New OAuth App"
3. Fill in:
   - **Application name**: "Visage Event Management"
   - **Homepage URL**: https://yourdomain.com
   - **Authorization callback URL**: `https://yourdomain.com/oauth/github/callback`
   - For development: `https://localhost:7400/oauth/github/callback`
4. Click "Register application"
5. Copy **Client ID** and **Client Secret**

### 2. Configure Application

#### Add OAuth Configuration to appsettings.json:
```json
{
  "OAuth": {
    "LinkedIn": {
      "ClientId": "YOUR_LINKEDIN_CLIENT_ID",
      "ClientSecret": "YOUR_LINKEDIN_CLIENT_SECRET"
    },
    "GitHub": {
      "ClientId": "YOUR_GITHUB_CLIENT_ID",
      "ClientSecret": "YOUR_GITHUB_CLIENT_SECRET"
    },
    "BaseUrl": "https://localhost:7400"   
  }
}
```

#### Optional: Configure `OAuth:BaseUrl`
- Use `OAuth:BaseUrl` when your frontend is accessed behind a proxy, or when provider callback URLs must match exactly (e.g., provider consoles that don't accept arbitrary host/port combinations).
- Example (development): `OAuth:BaseUrl = "https://localhost:7400"` → the server will build `https://localhost:7400/oauth/linkedin/callback` as the redirect_uri.
- This value overrides the request-derived base URL for redirect_uri generation and is useful when diagnosing provider "redirect_uri does not match" errors.
- The service logs the redirect URI at INFO level with keys `redirect_uri` and `usingConfiguredBase` to aid troubleshooting.

#### For Production (Environment Variables):
```bash
OAuth__LinkedIn__ClientId=your_linkedin_client_id
OAuth__LinkedIn__ClientSecret=your_linkedin_client_secret
OAuth__GitHub__ClientId=your_github_client_id
OAuth__GitHub__ClientSecret=your_github_client_secret
# Optional: base url for deterministic redirect_uri generation
OAuth__BaseUrl=https://your.domain
```

### 3. Database Schema Updates

The `Registrants` table already includes these fields:
```sql
-- OAuth verification tracking
IsLinkedInVerified BIT DEFAULT 0,
IsGitHubVerified BIT DEFAULT 0,
LinkedInVerifiedAt DATETIME2 NULL,
GitHubVerifiedAt DATETIME2 NULL,
LinkedInProfile NVARCHAR(500) NULL,
GitHubProfile NVARCHAR(500) NULL
```

### 4. Frontend Integration

#### Mandatory Registration Form:
- **For Employed/Self-Employed**: LinkedIn field shows with required indicator (`*`)
- **For Students**: GitHub field shows with required indicator (`*`)
- **For Unemployed**: Neither field is shown
- Validation enforces the appropriate social profile verification

## Security Features

### 1. State Token Validation
- Each OAuth flow generates a unique state token
- State is validated on callback to prevent CSRF attacks

### 2. Profile Ownership Verification
- Users must authenticate with the actual social platform
- We fetch profile data directly from the provider's API
- Profile URLs are extracted from verified API responses

### 3. Secure Token Handling
- OAuth access tokens are only used for profile verification
- Tokens are not stored - only the verified profile URL is saved
- All communications use HTTPS

### 4. Session Management
- State tokens are stored in server-side sessions
- Sessions are cleaned up after successful verification
- Failed attempts don't leave session debris

## Testing the Integration

### Development Testing:
1. Set up LinkedIn and GitHub OAuth apps with localhost callback URLs
2. Add OAuth configuration to `appsettings.Development.json`
3. Run the application: `dotnet run --project Visage.AppHost`
4. Navigate to `/registration/mandatory`
5. Select "Employed" and try connecting LinkedIn
6. Select "Student" and try connecting GitHub

### Verification Checklist:
- [ ] User is redirected to LinkedIn/GitHub for authentication
- [ ] After authorization, user is redirected back to the app
- [ ] Success message shows verified profile URL
- [ ] Profile is marked as verified in the database
- [ ] Timestamp is recorded for verification
- [ ] Form validation respects the verification status

## Error Handling

### Common Issues:
1. **"OAuth not configured"**: Missing ClientId/ClientSecret in configuration
2. **"Invalid OAuth state"**: State token mismatch (possible CSRF attempt)
3. **"Token exchange failed"**: Invalid client credentials or callback URL mismatch
4. **"Profile fetch failed"**: API permissions issue or rate limiting

### Debugging:
- Check application logs for OAuth flow details
- Verify callback URLs match exactly in OAuth app configuration
- Ensure client credentials are correct
- Check network requests in browser dev tools

## API Reference

### OAuth Initiation Endpoints:
- `GET /oauth/linkedin/start?returnUrl={url}` - Start LinkedIn OAuth
- `GET /oauth/github/start?returnUrl={url}` - Start GitHub OAuth

### OAuth Callback Endpoints:
- `GET /oauth/linkedin/callback?code={code}&state={state}` - LinkedIn callback
- `GET /oauth/github/callback?code={code}&state={state}` - GitHub callback

### Profile Management:
- `POST /api/profile/social/link-callback` - Store verified profile
- `GET /api/profile/social/status` - Get verification status

## Compliance & Privacy

### Data Collection:
- **Collected**: Profile URL, verification timestamp
- **NOT Collected**: Email addresses, contact lists, private profile data
- **Minimal Scopes**: Only request profile read permissions

### GDPR Compliance:
- Users can disconnect verified profiles
- Verification data is deleted when user account is deleted
- Clear consent is obtained before OAuth initiation

### Rate Limiting:
- LinkedIn: 100 requests per hour per app
- GitHub: 5,000 requests per hour per app
- Implementation includes retry logic and error handling

## Migration from Auth0 Social

### Changes Required:
1. Remove Auth0 social connection configuration
2. Update `SocialAuthService` to use direct OAuth endpoints
3. Update registration form to handle OAuth callbacks
4. Test all OAuth flows with new implementation

### Rollback Plan:
1. Keep Auth0 configuration as backup
2. Feature flag to switch between direct OAuth and Auth0
3. Database supports both verification methods
4. Gradual migration with user testing

## Support

### Troubleshooting OAuth Issues:
1. Check OAuth app configuration in provider consoles
2. Verify callback URLs match exactly
3. Confirm client credentials are correct
4. Review application logs for detailed error messages
5. Test with different browsers to rule out cookie/session issues