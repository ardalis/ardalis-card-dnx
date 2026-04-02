#!/usr/bin/env dotnet-script
using System.Diagnostics;

// Install reportgenerator into local tools folder (no global install required)
Run("dotnet", "tool update dotnet-reportgenerator-globaltool --tool-path tools");

const string coverageXml = "coverage/coverage.cobertura.xml";
Run("dotnet", $"test --project tests/Ardalis.Cli.Tests --coverage --coverage-output {coverageXml} --coverage-output-format cobertura");

// Find the generated coverage file (MTP puts it inside TestResults)
var coverageFile = Directory.GetFiles(".", "coverage.cobertura.xml", SearchOption.AllDirectories).FirstOrDefault();
if (coverageFile is null)
{
    Console.Error.WriteLine("No coverage.cobertura.xml found.");
    Environment.Exit(1);
}

var toolExe = OperatingSystem.IsWindows() ? @"tools\reportgenerator.exe" : "tools/reportgenerator";
Run(toolExe, $"-reports:\"{coverageFile}\" -targetdir:coverage-report -reporttypes:Html");

var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "coverage-report", "index.html");
if (File.Exists(reportPath))
{
    Console.WriteLine($"\nReport generated: {reportPath}");
    Run("cmd", $"/c start \"\" \"{reportPath}\"");
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
