# ardalis-card-dnx

[![NuGet](https://img.shields.io/nuget/v/ardalis.svg)](https://www.nuget.org/packages/ardalis)
[![NuGet Downloads](https://img.shields.io/nuget/dt/ardalis.svg)](https://www.nuget.org/packages/ardalis)

A .NET global tool that displays a personal business card in your terminal. 💠

## Overview

A CLI tool built for the .NET ecosystem using the new **`dnx`** command introduced in .NET 10! Run it instantly to access Steve "Ardalis" Smith's resources and information—no installation required!

## Quick Start with dnx

The easiest way to run this tool is with the new `dnx` command (similar to `npx` in Node.js):

```bash
dnx ardalis
```

That's it! The first time you run it, you'll be prompted to confirm the download. After that, it runs instantly without confirmation.

### Available Commands

View help and available commands:

```bash
dnx ardalis
```

Display the business card:

```bash
dnx ardalis card
```

Open Ardalis's blog:

```bash
dnx ardalis blog
```

Open Ardalis's Dometrain Author profile:

```bash
dnx ardalis dometrain
```

Open Ardalis's YouTube channel:

```bash
dnx ardalis youtube
```

Open Ardalis's Bluesky profile:

```bash
dnx ardalis bluesky
```

Open Ardalis's LinkedIn profile:

```bash
dnx ardalis linkedin
```

Open Ardalis's Pluralsight profile:

```bash
dnx ardalis pluralsight
```

Open Ardalis's GitHub profile:

```bash
dnx ardalis github
```

Open Ardalis's NuGet profile:

```bash
dnx ardalis nuget
```

Open Ardalis's newsletter subscription page:

```bash
dnx ardalis subscribe
```

Display a random quote:

```bash
dnx ardalis quote
```

Display popular GitHub repositories:

```bash
dnx ardalis repos
```

Display popular NuGet packages:

```bash
dnx ardalis packages
# or show all packages including sub-packages
dnx ardalis packages --all
```

Display published books:

```bash
dnx ardalis books
# Disable paging (show all books at once)
dnx ardalis books --no-paging
# Set page size (default: 10)
dnx ardalis books --page-size 5
```

Display recent activity across platforms:

```bash
dnx ardalis recent
# or use verbose mode to see detailed progress
dnx ardalis recent --verbose
```

Display a random coding tip:

```bash
dnx ardalis tip
```

Display available courses:

```bash
dnx ardalis courses
# Disable paging (show all courses at once)
dnx ardalis courses --no-paging
# Set page size (default: 10)
dnx ardalis courses --page-size 5
```

Display top videos from .NET Conf playlists:

```bash
dnx ardalis dotnetconf-score 2025
# or other years: 2024, 2023, 2022, 2021
dnx ardalis dotnetconf-score 2024
# save results as a markdown file
dnx ardalis dotnetconf-score 2025 --output results.md
dnx ardalis dotnetconf-score 2024 -o output.md
```

1. Go to [Google Cloud Console](https://console.cloud.google.com/apis/credentials)
2. Create a project (if needed)
3. Enable [YouTube Data API v3](https://console.cloud.google.com/apis/library/youtube.googleapis.com)
4. Create credentials (API key)

> **Future Enhancement:** This command currently requires users to provide their own YouTube API key. A better approach would be to use a dedicated service endpoint that handles third-party API interactions, removing the need for individual users to manage API keys and quotas.

Open NimblePros website:

```bash
dnx ardalis nimblepros
```

Open Ardalis's contact page:

```bash
dnx ardalis contact
```

Open Ardalis's Sessionize speaker profile:

```bash
dnx ardalis speaker
```

Check the version:

```bash
dnx ardalis --version
```

### Interactive Mode

Run in interactive mode to execute multiple commands without re-running the tool:

```bash
dnx ardalis -i
```

In interactive mode, simply type commands:

```text
> card
(displays card)

> quote
"New is glue." - Ardalis

> repos
(displays popular GitHub repositories with stars)

> books
(displays published books)

> blog
(opens blog)

> dometrain
(opens Dometrain Author profile)

> bluesky
(opens Bluesky profile)

> linkedin
(opens LinkedIn profile)

> exit
```

Exit by typing `exit`, `quit`, or pressing Enter on an empty line.

### Recent Activity Command

The `recent` command displays your latest activity from multiple platforms:

```bash
dnx ardalis recent
```

Use `--verbose` to see detailed progress from each source:

```bash
dnx ardalis recent --verbose
```

The verbose mode shows:

- ✅ Number of results found from each source
- ⚠️ Sources with no results
- ❌ Any errors encountered while fetching data

Activity is displayed with relative timestamps like "5 min ago" or "2 hours ago" for recent items, and short dates for older items.

## Permanent Installation

To install globally and run as just `ardalis` (without `dnx`):

```bash
dotnet tool install -g ardalis
```

Then run from anywhere:

```bash
ardalis            # Show help
ardalis card       # Display business card
ardalis blog       # Open blog
ardalis dometrain  # Open Dometrain Author profile
ardalis youtube    # Open YouTube channel
ardalis bluesky    # Open Bluesky profile
ardalis linkedin   # Open LinkedIn profile
ardalis pluralsight # Open Pluralsight profile
ardalis github     # Open GitHub profile
ardalis nuget      # Open NuGet profile
ardalis quote      # Display random quote
ardalis tip        # Display random coding tip
ardalis repos      # Display popular GitHub repositories
ardalis packages   # Display popular NuGet packages
ardalis books      # Display published books
ardalis courses    # Display available courses
ardalis courses --no-paging  # Display all courses at once
ardalis recent     # Display recent activity
ardalis dotnetconf-score 2025  # Display top .NET Conf videos by views
ardalis nimblepros # Open NimblePros website
ardalis contact    # Open contact page
ardalis speaker    # Open Sessionize speaker profile
ardalis subscribe  # Open newsletter subscription page
ardalis --version  # Check version
```

### Managing the Installation

Update to the latest version:

```bash
dotnet tool update -g ardalis
```

Uninstall:

```bash
dotnet tool uninstall -g ardalis
```

## Features

- ⚡ **One-command execution** with the new `dnx` command—no installation needed!
- 🎨 Beautiful terminal UI with [TimeWarp.Nuru](https://github.com/TimeWarpEngineering/timewarp-nuru)
- 💼 Quick access to professional links
- 🌐 Cross-platform (Windows, macOS, Linux)
- 🚀 Built with .NET 10.0

## dnx vs. Global Installation

**Key Differences:**

| Method | Command | Installation | Use Case |
|--------|---------|-------------|----------|
| **dnx** | `dnx ardalis` | None (downloads on first run) | Try it once, occasional use |
| **Global Tool** | `ardalis` | Permanent (`dotnet tool install -g`) | Frequent use, always available |

The `dnx` command is .NET's answer to Node.js's `npx`, introduced in .NET 10. It allows you to run .NET tools on-demand without explicitly installing them. Perfect for trying out tools or running one-off commands!

Learn more: [Running one-off .NET tools with dnx](https://andrewlock.net/exploring-dotnet-10-preview-features-5-running-one-off-dotnet-tools-with-dnx/)

## Repository Structure

```
ardalis-card-dnx/
├── src/
│   └── Ardalis.Cli/              # Main CLI tool project
│       ├── Ardalis.Cli.csproj
│       ├── Program.cs            # Route definitions (all commands registered here)
│       ├── Urls.cs               # Centralised URL constants
│       ├── ArdalisApiClient.cs   # HTTP client for api.ardalis.com
│       ├── NuGetVersionData.cs   # NuGet API response model
│       ├── Behaviors/            # TimeWarp.Nuru pipeline behaviors (e.g. telemetry)
│       ├── Endpoints/            # One file per command (IQuery handlers)
│       ├── Helpers/              # UrlHelper, QuoteHelper, TipHelper
│       └── Telemetry/            # OpenTelemetry / PostHog wiring
└── tests/
    └── Ardalis.Cli.Tests/        # TUnit test project
        ├── Ardalis.Cli.Tests.csproj
        ├── ArdalisApiClientConstructorTests.cs
        ├── ArdalisApiClientExtractPlaylistIdTests.cs
        ├── ParsePublicationYearTests.cs
        ├── UrlHelperAddUtmSourceTests.cs
        └── UrlHelperStripQueryStringTests.cs
```

## Building from Source

```bash
# Build everything
dotnet build

# Pack the tool
dotnet pack src/Ardalis.Cli/Ardalis.Cli.csproj

# Install locally from the packed output
dotnet tool install -g --add-source ./src/Ardalis.Cli/bin/Debug ardalis

# Or install and immediately test a specific command
dotnet run --project src/Ardalis.Cli -- card
dotnet run --project src/Ardalis.Cli -- --help
```

## Running Tests

Tests use [TUnit](https://github.com/thomhurst/TUnit) and are run with `dotnet run`:

```bash
dotnet run --project tests/Ardalis.Cli.Tests
```

**Do not** use `dotnet test` — TUnit on .NET 10 uses the Microsoft Testing Platform runner, which requires `dotnet run`.

## About

Created by Steve "Ardalis" Smith

- 🌐 [ardalis.com](https://ardalis.com)
- 🏢 [nimblepros.com](https://nimblepros.com)
- 📺 [YouTube](https://youtube.com/@Ardalis)

Specializing in Clean Architecture, Domain-Driven Design, and .NET development.

## Contributing

For maintainers: See [CONTRIBUTING.md](CONTRIBUTING.md) for instructions on publishing new versions.
