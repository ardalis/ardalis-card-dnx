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
        
        config.AddCommand<CardCommand>("card").WithDescription("Display Ardalis's business card.");
        config.AddCommand<BlogCommand>("blog").WithDescription("Open Ardalis's blog.");
        config.AddCommand<YouTubeCommand>("youtube").WithDescription("Open Ardalis's YouTube channel.");
        config.AddCommand<ContactCommand>("contact").WithDescription("Open Ardalis's contact page.");
        config.AddCommand<QuoteCommand>("quote").WithDescription("Display a random Ardalis quote.");
        config.AddCommand<ReposCommand>("repos").WithDescription("Display popular Ardalis GitHub repositories.");
        config.AddCommand<PackagesCommand>("packages").WithDescription("Display popular Ardalis NuGet packages.");
        config.AddCommand<BooksCommand>("books").WithDescription("Display published books by Ardalis.");
        config.AddCommand<TipsCommand>("tips").WithDescription("Display a random coding tip.");
        config.AddCommand<CoursesCommand>("courses").WithDescription("Display available courses.");
        
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
    
    config.AddCommand<CardCommand>("card")
        .WithDescription("Display Ardalis's business card.");

    config.AddCommand<BlogCommand>("blog")
        .WithDescription("Open Ardalis's blog.");

    config.AddCommand<YouTubeCommand>("youtube")
        .WithDescription("Open Ardalis's YouTube channel.");

    config.AddCommand<ContactCommand>("contact")
        .WithDescription("Open Ardalis's contact page.");

    config.AddCommand<QuoteCommand>("quote")
        .WithDescription("Display a random Ardalis quote.");

    config.AddCommand<ReposCommand>("repos")
        .WithDescription("Display popular Ardalis GitHub repositories.");

    config.AddCommand<PackagesCommand>("packages")
        .WithDescription("Display popular Ardalis NuGet packages.");

    config.AddCommand<BooksCommand>("books")
        .WithDescription("Display published books by Ardalis.");

    config.AddCommand<TipsCommand>("tips")
        .WithDescription("Display a random coding tip.");

    config.AddCommand<CoursesCommand>("courses")
        .WithDescription("Display available courses.");
        
    config.AddExample("card");
    config.AddExample("blog");
    config.AddExample("-i");
});

return app.Run(args);
