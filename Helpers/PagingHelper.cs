using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Ardalis.Helpers;

public static class PagingHelper
{
    public static void DisplayWithPaging<T>(
        IEnumerable<T> items,
        Action<T> displayAction,
        int pageSize = 10,
        bool enablePaging = true)
    {
        if (!enablePaging || pageSize <= 0)
        {
            // Display all items without paging
            foreach (var item in items)
            {
                displayAction(item);
            }
            return;
        }

        var itemsList = items as IList<T> ?? new List<T>(items);
        var totalItems = itemsList.Count;
        var currentIndex = 0;

        while (currentIndex < totalItems)
        {
            var endIndex = Math.Min(currentIndex + pageSize, totalItems);
            
            // Display current page of items
            for (int i = currentIndex; i < endIndex; i++)
            {
                displayAction(itemsList[i]);
            }

            currentIndex = endIndex;

            // Check if there are more items to display
            if (currentIndex < totalItems)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Markup("[dim]Press [bold]Space[/] for more, or any other key to exit...[/]");
                
                var key = Console.ReadKey(intercept: true);
                AnsiConsole.WriteLine(); // Clear the prompt line
                
                if (key.Key != ConsoleKey.Spacebar)
                {
                    AnsiConsole.MarkupLine("[dim]Showing {0} of {1} items[/]", currentIndex, totalItems);
                    break;
                }
            }
        }
    }

    public static void DisplayWithPaging<T>(
        IEnumerable<T> items,
        Func<T, Task> displayAction,
        int pageSize = 10,
        bool enablePaging = true)
    {
        if (!enablePaging || pageSize <= 0)
        {
            // Display all items without paging
            foreach (var item in items)
            {
                displayAction(item).GetAwaiter().GetResult();
            }
            return;
        }

        var itemsList = items as IList<T> ?? new List<T>(items);
        var totalItems = itemsList.Count;
        var currentIndex = 0;

        while (currentIndex < totalItems)
        {
            var endIndex = Math.Min(currentIndex + pageSize, totalItems);
            
            // Display current page of items
            for (int i = currentIndex; i < endIndex; i++)
            {
                displayAction(itemsList[i]).GetAwaiter().GetResult();
            }

            currentIndex = endIndex;

            // Check if there are more items to display
            if (currentIndex < totalItems)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.Markup("[dim]Press [bold]Space[/] for more, or any other key to exit...[/]");
                
                var key = Console.ReadKey(intercept: true);
                AnsiConsole.WriteLine(); // Clear the prompt line
                
                if (key.Key != ConsoleKey.Spacebar)
                {
                    AnsiConsole.MarkupLine("[dim]Showing {0} of {1} items[/]", currentIndex, totalItems);
                    break;
                }
            }
        }
    }
}
