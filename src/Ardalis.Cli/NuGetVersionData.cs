namespace Ardalis.Cli;

// Data class for NuGet version response
record NuGetVersionData([property: System.Text.Json.Serialization.JsonPropertyName("versions")] string[] Versions);
