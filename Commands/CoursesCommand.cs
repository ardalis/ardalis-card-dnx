using Ardalis.Cli.Telemetry;
using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class CoursesCommand : AsyncCommand<CoursesCommand.Settings>
{
    private readonly PostHogService _postHog;

    public CoursesCommand(PostHogService postHog)
    {
        _postHog = postHog;
    }

    public class Settings : CommandSettings
    {
        [CommandOption("--all")]
        [Description("Show all courses without paging")]
        public bool ShowAll { get; set; }

        [CommandOption("--page-size")]
        [Description("Sets page size (default: 10)")]
        public int PageSize { get; set; } = 10;
    }
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private const string CoursesJsonUrl = "https://ardalis.com/courses.json";

    static CoursesCommand()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken = default)
    {
        _postHog.TrackCommand("courses");
        AnsiConsole.MarkupLine("[bold green]Ardalis's Available Courses[/]\n");

        List<Course> courses;

        try
        {
            // Try to fetch courses from the URL
            courses = await FetchCoursesFromUrl();
        }
        catch
        {
            // Fallback to hard-coded courses if URL is unavailable
            AnsiConsole.MarkupLine("[dim]Using fallback course list...[/]\n");
            courses = GetFallbackCourses();
        }

        if (courses.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No courses available at the moment.[/]");
            return 0;
        }

        // Group courses by platform
        var coursesByPlatform = courses
            .GroupBy(c => c.Platform ?? "Other")
            .OrderBy(g => g.Key);

        // Flatten grouped courses for paging
        var allCoursesToDisplay = new List<(string Platform, Course Course)>();
        foreach (var platformGroup in coursesByPlatform)
        {
            foreach (var course in platformGroup)
            {
                allCoursesToDisplay.Add((platformGroup.Key, course));
            }
        }

        // Display courses with paging
        string currentPlatform = null;
        PagingHelper.DisplayWithPaging(
            allCoursesToDisplay,
            item =>
            {
                // Display platform header when it changes
                if (currentPlatform != item.Platform)
                {
                    if (currentPlatform != null)
                    {
                        AnsiConsole.WriteLine();
                    }
                    AnsiConsole.MarkupLine($"[bold cyan]{item.Platform}[/]");
                    AnsiConsole.WriteLine();
                    currentPlatform = item.Platform;
                }
                DisplayCourse(item.Course);
            },
            pageSize: settings.PageSize,
            enablePaging: !settings.ShowAll
        );

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Learn more at: [link]https://ardalis.com/courses[/][/]");

        return 0;
    }

    private static async Task<List<Course>> FetchCoursesFromUrl()
    {
        var response = await _httpClient.GetStringAsync(CoursesJsonUrl);
        var courses = JsonSerializer.Deserialize<List<Course>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return courses ?? new List<Course>();
    }

    private static List<Course> GetFallbackCourses()
    {
        return new List<Course>
        {
            new Course
            {
                Name = "SOLID Principles for C# Developers",
                Link = "https://www.pluralsight.com/courses/csharp-solid-principles",
                Platform = "Pluralsight",
                Description = "Learn the SOLID principles of object-oriented design and how to apply them in C# to write maintainable, flexible code."
            },
            new Course
            {
                Name = "Getting Started: Modular Monoliths in .NET",
                Link = "https://dometrain.com/course/getting-started-modular-monoliths-in-dotnet/?ref=steve-ardalis-smith&coupon_code=ARDALIS",
                Platform = "Dometrain",
                Description = "A modular monolith breaks up the application into logical modules which are largely independent from one another. This provides many of the benefits of more distributed approaches like microservices without the overhead of deploying and managing a distributed application."
            }
        };
    }

    private static void DisplayCourse(Course course)
    {
        var urlWithTracking = UrlHelper.AddUtmSource(course.Link);
        var displayUrl = UrlHelper.StripQueryString(course.Link);

        var panel = new Panel(new Markup(
            $"[bold]{course.Name}[/]\n\n" +
            $"{(string.IsNullOrEmpty(course.Description) ? "[dim]No description available[/]" : course.Description)}\n\n" +
            $"[dim]Learn more:[/] [link={urlWithTracking}]{displayUrl}[/]"
        ))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private class Course
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("platform")]
        public string Platform { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }
}
