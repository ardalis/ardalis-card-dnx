using Ardalis;
using Ardalis.Commands;
using Spectre.Console;
using Spectre.Console.Cli;

// Check for interactive mode
if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
{
    return await InteractiveMode.RunAsync();
}

// Check for help flag to add custom installation instructions
if (args.Length > 0 && (args[0] == "-h" || args[0] == "--help" || args[0] == "help"))
{
    var helpApp = new CommandApp();
    helpApp.Configure(config =>
    {
        config.SetApplicationName("ardalis");
        config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");
        
        // Display commands (alphabetical)
        config.AddCommand<BooksCommand>("books").WithDescription("Display published books by Ardalis.");
        config.AddCommand<CardCommand>("card").WithDescription("Display Ardalis's business card.");
        config.AddCommand<CoursesCommand>("courses").WithDescription("Display available courses.");
        config.AddCommand<PackagesCommand>("packages").WithDescription("Display popular Ardalis NuGet packages.");
        config.AddCommand<QuoteCommand>("quote").WithDescription("Display a random Ardalis quote.");
        config.AddCommand<RecentCommand>("recent").WithDescription("Display recent activity from Ardalis.");
        config.AddCommand<ReposCommand>("repos").WithDescription("Display popular Ardalis GitHub repositories.");
        config.AddCommand<TipsCommand>("tips").WithDescription("Display a random coding tip.");
        
        // Open commands (alphabetical)
        config.AddCommand<BlogCommand>("blog").WithDescription("Open Ardalis's blog.");
        config.AddCommand<BlueSkyCommand>("bluesky").WithDescription("Open Ardalis's Bluesky profile.");
        config.AddCommand<ContactCommand>("contact").WithDescription("Open Ardalis's contact page.");
        config.AddCommand<LinkedInCommand>("linkedin").WithDescription("Open Ardalis's LinkedIn profile.");
        config.AddCommand<SpeakerCommand>("speaker").WithDescription("Open Ardalis's Sessionize speaker profile.");
        config.AddCommand<YouTubeCommand>("youtube").WithDescription("Open Ardalis's YouTube channel.");
        
        config.AddExample("card");
        config.AddExample("blog");
        config.AddExample("-i");
    });
    
    var result = helpApp.Run(args);
    
    // Add installation instructions after the help output
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("[bold]INSTALLATION:[/]");
    AnsiConsole.MarkupLine("  Install as a global .NET tool:");
    AnsiConsole.MarkupLine("    [cyan]dotnet tool install -g ardalis[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("  Update to the latest version:");
    AnsiConsole.MarkupLine("    [cyan]dotnet tool update -g ardalis[/]");
    AnsiConsole.WriteLine();
    AnsiConsole.MarkupLine("  [dim]Once installed, use 'ardalis' instead of 'dnx ardalis'[/]");
    
    return result;
}

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("ardalis");
    config.SetApplicationVersion(typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");
    
    // Display commands (alphabetical)
    config.AddCommand<BooksCommand>("books")
        .WithDescription("Display published books by Ardalis.");

    config.AddCommand<CardCommand>("card")
        .WithDescription("Display Ardalis's business card.");

    config.AddCommand<CoursesCommand>("courses")
        .WithDescription("Display available courses.");

    config.AddCommand<PackagesCommand>("packages")
        .WithDescription("Display popular Ardalis NuGet packages.");

    config.AddCommand<QuoteCommand>("quote")
        .WithDescription("Display a random Ardalis quote.");

    config.AddCommand<RecentCommand>("recent")
        .WithDescription("Display recent activity from Ardalis.");

    config.AddCommand<ReposCommand>("repos")
        .WithDescription("Display popular Ardalis GitHub repositories.");

    config.AddCommand<TipsCommand>("tips")
        .WithDescription("Display a random coding tip.");

    // Open commands (alphabetical)
    config.AddCommand<BlogCommand>("blog")
        .WithDescription("Open Ardalis's blog.");

    config.AddCommand<BlueSkyCommand>("bluesky")
        .WithDescription("Open Ardalis's Bluesky profile.");

    config.AddCommand<ContactCommand>("contact")
        .WithDescription("Open Ardalis's contact page.");

    config.AddCommand<LinkedInCommand>("linkedin")
        .WithDescription("Open Ardalis's LinkedIn profile.");

    config.AddCommand<SpeakerCommand>("speaker")
        .WithDescription("Open Ardalis's Sessionize speaker profile.");
  
    config.AddCommand<YouTubeCommand>("youtube")
        .WithDescription("Open Ardalis's YouTube channel.");
        
    config.AddExample("card");
    config.AddExample("blog");
    config.AddExample("-i");
});

return app.Run(args);
