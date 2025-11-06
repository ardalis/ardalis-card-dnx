using Ardalis;
using Ardalis.Commands;
using Spectre.Console.Cli;

// Check for interactive mode
if (args.Length > 0 && (args[0] == "-i" || args[0] == "--interactive"))
{
    return await InteractiveMode.RunAsync();
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

    config.AddCommand<QuoteCommand>("quote")
        .WithDescription("Display a random Ardalis quote.");

    config.AddCommand<ReposCommand>("repos")
        .WithDescription("Display popular Ardalis GitHub repositories.");

    config.AddCommand<BooksCommand>("books")
        .WithDescription("Display published books by Ardalis.");
        
    config.AddExample("card");
    config.AddExample("blog");
    config.AddExample("-i");
});

return app.Run(args);
