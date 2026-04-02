#!/usr/bin/env dotnet-script
using System.Diagnostics;

bool isCI = Environment.GetEnvironmentVariable("CI") == "true";

if (!isCI)
    Run("dotnet", "tool update dotnet-reportgenerator-globaltool --tool-path tools");

Run("dotnet", "test --project tests/Ardalis.Cli.Tests --coverage --coverage-output coverage/coverage.cobertura.xml --coverage-output-format cobertura --coverage-settings coverage.settings");

// MTP places coverage relative to the test binary's TestResults folder - copy to well-known location
var coverageFile = Directory.GetFiles(".", "coverage.cobertura.xml", SearchOption.AllDirectories).FirstOrDefault();
if (coverageFile is null)
{
    Console.Error.WriteLine("No coverage.cobertura.xml found.");
    Environment.Exit(1);
}

const string coverageXml = "coverage/coverage.cobertura.xml";
Directory.CreateDirectory("coverage");
File.Copy(coverageFile, coverageXml, overwrite: true);
Console.WriteLine($"Coverage XML: {Path.GetFullPath(coverageXml)}");

if (!isCI)
{
    var toolExe = OperatingSystem.IsWindows() ? @"tools\reportgenerator.exe" : "tools/reportgenerator";
    Run(toolExe, $"-reports:\"{coverageXml}\" -targetdir:coverage-report -reporttypes:Html");

    var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "coverage-report", "index.html");
    if (File.Exists(reportPath))
    {
        Console.WriteLine($"\nReport generated: {reportPath}");
        Run("cmd", $"/c start \"\" \"{reportPath}\"");
    }
}

static void Run(string command, string args)
{
    Console.WriteLine($"\n> {command} {args}\n");
    var psi = new ProcessStartInfo(command, args)
    {
        UseShellExecute = false,
        RedirectStandardOutput = false,
        RedirectStandardError = false,
    };
    var process = Process.Start(psi)!;
    process.WaitForExit();
    if (process.ExitCode != 0)
        Environment.Exit(process.ExitCode);
}
