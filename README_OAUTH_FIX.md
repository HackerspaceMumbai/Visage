# ?? OAuth Fix Summary

## The Problem
When you click "Connect LinkedIn", it still goes to Auth0 instead of your LinkedIn OAuth app.

## Root Cause
The `/Account/LinkSocial` endpoint in `Program.cs` still uses Auth0. We created the OAuth infrastructure but didn't wire up the endpoints.

## The Fix (5 minutes)
Follow the instructions in **`QUICK_FIX_OAUTH.md`** to:
1. Add `using Visage.FrontEnd.Web.Configuration;`
2. Add session support and DirectOAuthService registration
3. Add `app.UseSession();` middleware
4. Replace `/Account/LinkSocial` with 4 new OAuth endpoints

## Files to Edit
- `Visage.FrontEnd.Web/Program.cs` (only file that needs changes)

## After the Fix
? LinkedIn button ? YOUR LinkedIn app
? GitHub button ? YOUR GitHub app  
? Profile verification works correctly
? No more Auth0 social connections

## Setup OAuth Apps
Before testing, make sure you've:
1. Created LinkedIn OAuth app with callback: `https://localhost:7400/oauth/linkedin/callback`
2. Created GitHub OAuth app with callback: `https://localhost:7400/oauth/github/callback`
3. Run `./setup-oauth-secrets.bat` and set your credentials

See `Visage.FrontEnd.Web/Configuration/oauth-setup.md` for detailed setup instructions.

## Quick Test
```bash
# 1. Make the changes to Program.cs
# 2. Rebuild
dotnet build

# 3. Run
cd Visage.AppHost
dotnet run

# 4. Navigate to /registration/mandatory
# 5. Select "Employed"
# 6. Click "Connect LinkedIn"
# 7. Should redirect to LinkedIn.com (not Auth0!)
```

## Need Help?
- Full instructions: `QUICK_FIX_OAUTH.md`
- Setup guide: `Visage.FrontEnd.Web/Configuration/oauth-setup.md`
- Complete documentation: `docs/Direct-OAuth-Profile-Verification.md`