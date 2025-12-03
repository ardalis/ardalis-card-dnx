# Add Missing GitHub and NuGet Commands to README

## Summary

Add documentation for the `github` and `nuget` commands which exist in Program.cs but are missing from README.md.

## Todo List

- [x] Add `github` command to Quick Start section (after `pluralsight`)
- [x] Add `nuget` command to Quick Start section (after `github`)
- [x] Add `github` command to Permanent Installation examples
- [x] Add `nuget` command to Permanent Installation examples

## Results

Added both missing commands to README.md:
- Quick Start section: Added after pluralsight, before subscribe
- Permanent Installation: Added after pluralsight, before quote

## Notes

**Missing routes discovered during documentation audit:**

From Program.cs:
```csharp
.Map("github", () => Open(GitHub), "Open Ardalis's GitHub profile")
.Map("nuget", () => Open(NuGet), "Open Ardalis's NuGet profile")
```

**Quick Start format to follow:**
```markdown
Open Ardalis's GitHub profile:

```bash
dnx ardalis github
```

Open Ardalis's NuGet profile:

```bash
dnx ardalis nuget
```
```

**Permanent Installation format:**
```
ardalis github     # Open GitHub profile
ardalis nuget      # Open NuGet profile
```
