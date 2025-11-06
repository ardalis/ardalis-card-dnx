using Spectre.Console;
using System;
using System.Diagnostics;

namespace Ardalis.Helpers;

public static class UrlHelper
{
    public static void Open(string url)
    {
        try
        {
            using var ps = new Process();
            ps.StartInfo = new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            ps.Start();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Failed to open URL:[/] {ex.Message}");
        }
    }
}
