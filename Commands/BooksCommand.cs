using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Ardalis.Commands;

public class BooksCommand : AsyncCommand
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private const string BooksJsonUrl = "https://ardalis.com/books.json";

    static BooksCommand()
    {
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    public override async Task<int> ExecuteAsync(CommandContext context)
    {
        AnsiConsole.MarkupLine("[bold blue]Ardalis's Published Books[/]\n");

        List<Book> books;
        
        try
        {
            // Try to fetch books from the URL
            books = await FetchBooksFromUrl();
        }
        catch
        {
            // Fallback to hard-coded book if URL is unavailable
            AnsiConsole.MarkupLine("[dim]Using fallback book list...[/]\n");
            books = GetFallbackBooks();
        }

        if (books.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No books available at the moment.[/]");
            return 0;
        }

        // Sort books by publication date (most recent first)
        var sortedBooks = books
            .OrderByDescending(b => ParsePublicationYear(b.PublicationDate))
            .ToList();

        // Display books
        foreach (var book in sortedBooks)
        {
            DisplayBook(book);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[dim]Learn more at: [link]https://ardalis.com/books[/][/]");

        return 0;
    }

    private static async Task<List<Book>> FetchBooksFromUrl()
    {
        var response = await _httpClient.GetStringAsync(BooksJsonUrl);
        var books = JsonSerializer.Deserialize<List<Book>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return books ?? new List<Book>();
    }

    private static List<Book> GetFallbackBooks()
    {
        return new List<Book>
        {
            new Book
            {
                Title = "Architecting Modern Web Applications with ASP.NET Core and Microsoft Azure",
                Link = "https://ardalis.com/architecture-ebook/",
                CoverImage = "https://ardalis.com/img/Architecture-eBook-Cover-242x300.png",
                Description = "Learn how to architect modern web applications using ASP.NET Core and Azure. This free eBook covers architecture patterns, clean code practices, and cloud deployment strategies.",
                Publisher = "Microsoft",
                PublicationDate = "2023"
            }
        };
    }

    private static void DisplayBook(Book book)
    {
        var panel = new Panel(new Markup(
            $"[bold]{book.Title}[/]\n\n" +
            $"{(string.IsNullOrEmpty(book.Description) ? "[dim]No description available[/]" : book.Description)}\n\n" +
            $"[dim]Publisher:[/] {(string.IsNullOrEmpty(book.Publisher) ? "N/A" : book.Publisher)}\n" +
            $"[dim]Published:[/] {(string.IsNullOrEmpty(book.PublicationDate) ? "N/A" : book.PublicationDate)}\n\n" +
            $"[link={book.Link}]Read More →[/]"
        ))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue),
            Padding = new Padding(1, 0, 1, 0)
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    private static int ParsePublicationYear(string publicationDate)
    {
        if (string.IsNullOrWhiteSpace(publicationDate))
            return 0;

        // Try to parse as a year directly
        if (int.TryParse(publicationDate, out int year))
            return year;

        // Try to extract year from various date formats
        if (DateTime.TryParse(publicationDate, out DateTime date))
            return date.Year;

        // If all else fails, try to find a 4-digit year in the string
        var match = System.Text.RegularExpressions.Regex.Match(publicationDate, @"\b(19|20)\d{2}\b");
        if (match.Success && int.TryParse(match.Value, out int extractedYear))
            return extractedYear;

        return 0;
    }

    private class Book
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("link")]
        public string Link { get; set; } = string.Empty;

        [JsonPropertyName("coverImage")]
        public string CoverImage { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("publisher")]
        public string Publisher { get; set; } = string.Empty;

        [JsonPropertyName("publicationDate")]
        public string PublicationDate { get; set; } = string.Empty;

        [JsonPropertyName("isbn")]
        public string Isbn { get; set; } = string.Empty;
    }
}
