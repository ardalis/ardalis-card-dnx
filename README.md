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

Open Ardalis's Dometrain profile:

```bash
dnx ardalis dometrain
```

Open Ardalis's YouTube channel:

```bash
dnx ardalis youtube
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
```

Display recent activity across platforms:

```bash
dnx ardalis recent
# or use verbose mode to see detailed progress
dnx ardalis recent --verbose
```

Display a random coding tip:

```bash
dnx ardalis tips
```

Display available courses:

```bash
dnx ardalis courses
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
(opens Dometrain profile)

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
ardalis dometrain  # Open Dometrain profile
ardalis youtube    # Open YouTube channel
ardalis quote      # Display random quote
ardalis repos      # Display popular GitHub repositories
ardalis packages   # Display popular NuGet packages
ardalis books      # Display published books
ardalis recent     # Display recent activity across platforms
ardalis tips       # Display a random coding tip
ardalis courses    # Display available courses
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
- 🎨 Beautiful terminal UI with [Spectre.Console](https://spectreconsole.net/)
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

## Building from Source

```bash
dotnet build
dotnet pack
dotnet tool install -g --add-source ./bin/Debug ardalis
```

## About

Created by Steve "Ardalis" Smith

- 🌐 [ardalis.com](https://ardalis.com)
- 🏢 [nimblepros.com](https://nimblepros.com)
- 📺 [YouTube](https://youtube.com/@Ardalis)

Specializing in Clean Architecture, Domain-Driven Design, and .NET development.

## Contributing

For maintainers: See [CONTRIBUTING.md](CONTRIBUTING.md) for instructions on publishing new versions.
