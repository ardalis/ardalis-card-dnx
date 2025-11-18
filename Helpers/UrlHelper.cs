using Spectre.Console;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ardalis.Helpers;

public static class UrlHelper
{
    public static void Open(string url)
    {
        AnsiConsole.MarkupLine($"Opening [link={url}]{url}[/]");

        try
        {
            var opened = TryOpenUrl(url);
            if (!opened)
            {
                // Fallback: display the URL as a clickable link
                AnsiConsole.MarkupLine($"[dim]Open in your browser:[/] [link={url}]{url}[/]");
            }
        }
        catch
        {
            AnsiConsole.MarkupLine($"[yellow]Could not open browser automatically.[/]");
            AnsiConsole.MarkupLine($"[dim]Please visit:[/] [link={url}]{url}[/]");
        }
    }

    private static bool TryOpenUrl(string url)
    {
        try
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Check if running in WSL
                if (IsWsl())
                {
                    return TryStartProcess("wslview", url) || TryStartProcess("explorer.exe", url);
                }

                // Native Windows
                using var ps = Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
                return ps != null;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Try common Linux browser openers
                return TryStartProcess("xdg-open", url)
                    || TryStartProcess("sensible-browser", url)
                    || TryStartProcess("gnome-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return TryStartProcess("open", url);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryStartProcess(string command, string arguments)
    {
        try
        {
            using var ps = Process.Start(new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });
            return ps != null;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsWsl()
    {
        try
        {
            // Check if /proc/version contains "microsoft" or "WSL"
            if (System.IO.File.Exists("/proc/version"))
            {
                var version = System.IO.File.ReadAllText("/proc/version");
                return version.Contains("microsoft", StringComparison.OrdinalIgnoreCase)
                    || version.Contains("WSL", StringComparison.OrdinalIgnoreCase);
            }
        }
        catch
        {
            // Ignore errors
        }
        return false;
    }
}
