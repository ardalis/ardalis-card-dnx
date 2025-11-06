using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class RecentCommand : AsyncCommand<RecentCommand.Settings>
{
    public class Settings : CommandSettings
    {
        [CommandOption("--verbose")]
        [Description("Show detailed progress for each source")]
        public bool Verbose { get; set; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[bold]Fetching recent activity...[/]");
        AnsiConsole.WriteLine();
        
        var activities = await RecentHelper.GetRecentActivitiesAsync(settings.Verbose);
        
        if (activities.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No recent activities found.[/]");
            return 0;
        }

        var table = new Table();
        table.Border = TableBorder.Rounded;
        table.BorderStyle = new Style(Color.Blue);
        
        table.AddColumn(new TableColumn("[bold]Source[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Activity[/]").LeftAligned());
        table.AddColumn(new TableColumn("[bold]When[/]").RightAligned());
        table.AddColumn(new TableColumn("[bold]Link[/]").Centered());
        
        foreach (var activity in activities)
        {
            var truncatedTitle = activity.GetTruncatedTitle(60);
            var sourceWithIcon = $"{activity.Icon} {activity.Source}";
            var when = activity.GetRelativeTimeString();
            var link = $"[link={activity.Url}]Click for details[/]";
            
            table.AddRow(sourceWithIcon, truncatedTitle, when, link);
        }
        
        AnsiConsole.Write(table);
        
        return 0;
    }
}
