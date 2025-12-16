#!/bin/bash

# OAuth User Secrets Setup Script for Visage
# This script initializes user secrets and provides commands to set OAuth credentials

echo "?? Setting up OAuth User Secrets for Visage"
echo "============================================="

# Navigate to the web project directory
cd "Visage.FrontEnd.Web" || exit 1

# Initialize user secrets if not already done
echo "?? Initializing user secrets..."
dotnet user-secrets init

echo ""
echo "? User secrets initialized!"
echo ""
echo "?? Next, set your OAuth credentials using these commands:"
echo ""
echo "?? LinkedIn OAuth:"
echo "   dotnet user-secrets set \"OAuth:LinkedIn:ClientId\" \"YOUR_LINKEDIN_CLIENT_ID\""
echo "   dotnet user-secrets set \"OAuth:LinkedIn:ClientSecret\" \"YOUR_LINKEDIN_CLIENT_SECRET\""
echo ""
echo "?? GitHub OAuth:"  
echo "   dotnet user-secrets set \"OAuth:GitHub:ClientId\" \"YOUR_GITHUB_CLIENT_ID\""
echo "   dotnet user-secrets set \"OAuth:GitHub:ClientSecret\" \"YOUR_GITHUB_CLIENT_SECRET\""
echo ""
echo "?? To verify your secrets are set:"
echo "   dotnet user-secrets list"
echo ""
echo "?? Don't have OAuth apps yet? Follow these guides:"
echo "   LinkedIn: https://www.linkedin.com/developers/apps"
echo "   GitHub: https://github.com/settings/developers"
echo ""
echo "?? For detailed setup instructions, see:"
echo "   Visage.FrontEnd.Web/Configuration/oauth-setup.md"
echo "   docs/Direct-OAuth-Profile-Verification.md"