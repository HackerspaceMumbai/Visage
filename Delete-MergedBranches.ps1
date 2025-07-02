# Requires GitHub CLI (gh) and git
# Run this script in your repo directory

# Fetch latest from origin
git fetch --prune

# Get list of merged PRs and their head branches
$mergedBranches = gh pr list --state merged --json headRefName --jq '.[].headRefName'

# Split into array
$branches = $mergedBranches -split "`n"

foreach ($branch in $branches) {
    # Skip empty lines and protected branches
    if ($branch -and $branch -ne "main" -and $branch -ne "master") {
        # Check if branch exists locally
        if (git show-ref --verify --quiet "refs/heads/$branch") {
            Write-Host "Deleting local branch: $branch"
            git branch -d $branch
        }
        # Check if branch exists on remote
        $remoteBranchExists = git ls-remote --exit-code --heads origin $branch 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Deleting remote branch: $branch"
            git push origin --delete $branch
        }
    }
}

Write-Host "Cleanup complete."