#nullable enable
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using TimeWarp.Terminal;

namespace Ardalis.Helpers;

public static class UrlHelper
{
    // Seam for unit testing: replace to suppress real process launches.
    // Production code always uses Process.Start; tests can substitute a no-op.
    internal static Func<ProcessStartInfo, Process?> ProcessStart { get; set; } =
        psi => Process.Start(psi);
    /// <summary>
    /// Removes query string parameters from a URL for display purposes
    /// </summary>
    public static string StripQueryString(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        var queryIndex = url.IndexOf('?');
        return queryIndex >= 0 ? url.Substring(0, queryIndex) : url;
    }

    /// <summary>
    /// Adds utm_source=ardaliscli to a URL if not already present
    /// </summary>
    public static string AddUtmSource(string url)
    {
        if (string.IsNullOrEmpty(url))
            return url;

        // Check if utm_source is already present
        if (url.Contains("utm_source=", StringComparison.OrdinalIgnoreCase))
            return url;

        // Determine if we need ? or &
        var separator = url.Contains('?') ? "&" : "?";
        return $"{url}{separator}utm_source=ardaliscli";
    }

    public static void Open(string url)
    {
        ITerminal terminal = TimeWarpTerminal.Default;

        // Add UTM source for tracking
        string urlWithTracking = AddUtmSource(url);

        // Display URL without query parameters for cleaner output
        string displayUrl = StripQueryString(url);
        terminal.WriteLine("Opening " + displayUrl.Link(urlWithTracking));

        try
        {
            bool opened = TryOpenUrl(urlWithTracking);
            if (!opened)
            {
                // Fallback: display the URL as a clickable link
                terminal.WriteLine("Open in your browser: ".Gray() + displayUrl.Link(urlWithTracking).Cyan());
            }
        }
        catch
        {
            terminal.WriteLine("Could not open browser automatically.".Yellow());
            terminal.WriteLine("Please visit: ".Gray() + displayUrl.Link(urlWithTracking).Cyan());
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
                using var ps = ProcessStart(new ProcessStartInfo
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
            using var ps = ProcessStart(new ProcessStartInfo
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
