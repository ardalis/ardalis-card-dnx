using Ardalis.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;
using System.Threading;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class RecentCommand : AsyncCommand
{
    public override async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        AnsiConsole.MarkupLine("[bold]Fetching recent activity...[/]");
        AnsiConsole.WriteLine();
        
        var activities = await RecentHelper.GetRecentActivitiesAsync();
        
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
        table.AddColumn(new TableColumn("[bold]Link[/]").Centered());
        
        foreach (var activity in activities)
        {
            var truncatedTitle = activity.GetTruncatedTitle(60);
            var sourceWithIcon = $"{activity.Icon} {activity.Source}";
            var link = $"[link={activity.Url}]Click for details[/]";
            
            table.AddRow(sourceWithIcon, truncatedTitle, link);
        }
        
        AnsiConsole.Write(table);
        
        return 0;
    }
}
