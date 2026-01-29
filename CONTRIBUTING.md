# Hi 👋 Welcome to Visage's contributing guide <!-- omit in toc -->

👍🎉 First off, thanks for taking the time to contribute! 🎉👍

>IMP: Do not open up a GitHub issue if the bug is a security vulnerability, please refer to our [security policy](./SECURITY.md).

Visage is being developed in the open, and is constantly being improved by our users, contributors, and maintainers. It is because of you🫵 that we can bring great software to the community, and make this project successful.

This guide provides information on filing issues and guidelines for open source contributors. Please leave comments / suggestions if you find something is missing or incorrect.

## Table Of Contents

[Code of Conduct](#code-of-conduct)

[I don't want to read this whole thing, I just have a question!!!](#i-dont-want-to-read-this-whole-thing-i-just-have-a-question)

[Get Started](#get-started)

[How Can I Contribute?](#how-can-i-contribute)

[Testing (TUnit)](#testing-tunit)

## Code of Conduct

Keep our community approachable and respectable. This project and everyone participating in it is governed by the [Hackerspace Mumbai's Code of Conduct](https://github.com/HackerspaceMumbai/HackerspaceMumbai/blob/master/CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to community@hackmum.in.

## If you just have a question?

> **Note:** Please don't file an issue to ask a question.

We have an official message board with a detailed FAQ and where the community chimes in with helpful advice if you have questions.

[Github Discussions, the official Visage message board](https://github.com/HackerspaceMumbai/Visage/discussions)

or you can begin a conversation on our social media: 
- [Twitter/X](https://twitter.com/hackmum)
- [Mastodon](https://hachyderm.io/@hackmum)
- [Facebook](https://www.facebook.com/hackmum)

## Get Started

To get an overview of the project, your first stop should be our [README](./README.md). Check out the [project wiki](https://github.com/HackerspaceMumbai/Visage/wiki) for the overall design and architecture related information.

TODO: Depict and explain Repository structure

## How Can I Contribute?

You need to have rudimentary git and [GitHub](https://docs.github.com/en/get-started/exploring-projects-on-github/finding-ways-to-contribute-to-open-source-on-github) knowledge

### Reporting Issues

We have two types of GitHub issues:

1. [Bug Reports](.github/ISSUE_TEMPLATE/bug_report.md)
2. [Feature Requests](.github/ISSUE_TEMPLATE/feature_request.md)

Following the guidelines as mentioned in the respective template will help maintainers and the community understand your report 📝, reproduce the behavior 💻🖥️, and find related issues 🔍.

> **Note:** If you find a **Closed** issue that seems like it is the same thing that you're experiencing, open a new issue and include a link to the original issue in the body of your new one.

### Make Changes

PR are always welcome, even if they only contain small fixes like typos or a few lines of code. But, please document it as an issue as per the [PR template](./pull_request_template.md) , get a discussion going, and ensure a maintainer has approved the go-ahead before starting to work on it.

If its hacktoberfest related, the maintainer will suitably label the PR as hacktoberfest.

## Testing (TUnit)

Visage uses **TUnit** for automated tests.

### Running tests

```powershell
# Run all tests

dotnet test
```

### Selecting tests (TUnit `--treenode-filter`)

TUnit supports selecting tests using `--treenode-filter`.

Filter format: `/<Assembly>/<Namespace>/<Class name>/<Test name>` (wildcards supported with `*`).

```powershell
# Run all tests in a class

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/HealthEndpointTests/*"

# Run a single test by name

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/All_Http_Resources_Should_Have_Health_Endpoints"
```

### Selecting by category

Many tests use `[Category("...")]` (for example `RequiresAuth`, `AspireHealth`, `E2E`).

TUnit supports property-based filters:

```powershell
# Run RequiresAuth tests

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/*[Category=RequiresAuth]"

# Exclude RequiresAuth tests

dotnet test --project tests\Visage.Test.Aspire\Visage.Test.Aspire.csproj --treenode-filter "/*/*/*/*[Category!=RequiresAuth]"
```

> **Note:** Avoid vstest `--filter` guidance for TUnit runs; prefer `--treenode-filter`.

**Looking forward to your contribution 🙏**
