#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Handlers;

/// <summary>
/// Displays published books using Nuru panel widget with paging support.
/// </summary>
public static class BooksHandler
{
    private const string BooksJsonUrl = "https://ardalis.com/books.json";
    private const string BooksPageUrl = "https://ardalis.com/books";

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    static BooksHandler()
    {
        HttpClient.DefaultRequestHeaders.Add("User-Agent", "ardalis-cli");
    }

    private static readonly Book[] FallbackBooks =
    [
        new Book
        {
            Title = "Architecting Modern Web Applications with ASP.NET Core and Microsoft Azure",
            Link = "https://ardalis.com/architecture-ebook/",
            CoverImage = "https://ardalis.com/img/Architecture-eBook-Cover-242x300.png",
            Description = "Learn how to architect modern web applications using ASP.NET Core and Azure. This free eBook covers architecture patterns, clean code practices, and cloud deployment strategies.",
            Publisher = "Microsoft",
            PublicationDate = "2023"
        }
    ];

    public static async Task ExecuteAsync(bool noPaging, int pageSize)
    {
        ITerminal terminal = NuruTerminal.Default;

        terminal.WriteLine("Ardalis's Published Books".Blue().Bold());
        terminal.WriteLine();

        List<Book> books;

        try
        {
            books = await FetchBooksFromUrlAsync();
        }
        catch
        {
            terminal.WriteLine("Using fallback book list...".Gray());
            terminal.WriteLine();
            books = [.. FallbackBooks];
        }

        if (books.Count == 0)
        {
            terminal.WriteLine("No books available at the moment.".Yellow());
            return;
        }

        // Sort books by publication date (most recent first)
        List<Book> sortedBooks = books
            .OrderByDescending(b => ParsePublicationYear(b.PublicationDate))
            .ToList();

        if (noPaging)
        {
            // Display all books without paging
            foreach (Book book in sortedBooks)
            {
                DisplayBook(terminal, book);
            }
        }
        else
        {
            // Display books with paging
            int currentIndex = 0;

            while (currentIndex < sortedBooks.Count)
            {
                int endIndex = Math.Min(currentIndex + pageSize, sortedBooks.Count);
                List<Book> pageBooks = sortedBooks.Skip(currentIndex).Take(endIndex - currentIndex).ToList();

                foreach (Book book in pageBooks)
                {
                    DisplayBook(terminal, book);
                }

                currentIndex = endIndex;

                if (currentIndex < sortedBooks.Count)
                {
                    terminal.Write("Press ".Gray());
                    terminal.Write("Space".Bold());
                    terminal.Write(" for more, or any other key to exit...".Gray());

                    ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                    terminal.WriteLine();

                    if (key.Key != ConsoleKey.Spacebar)
                    {
                        terminal.WriteLine($"Showing {currentIndex} of {sortedBooks.Count} books".Gray());
                        break;
                    }
                    terminal.WriteLine();
                }
            }
        }

        terminal.WriteLine();
        string booksUrl = AddUtmSource(BooksPageUrl);
        terminal.WriteLine("Learn more at: ".Gray() + BooksPageUrl.Link(booksUrl).Cyan());
    }

    private static void DisplayBook(ITerminal terminal, Book book)
    {
        string urlWithTracking = AddUtmSource(book.Link);
        string displayUrl = StripQueryString(book.Link);

        string description = string.IsNullOrEmpty(book.Description)
            ? "No description available".Gray()
            : book.Description;

        string publisher = string.IsNullOrEmpty(book.Publisher)
            ? "N/A"
            : book.Publisher;

        string publicationDate = string.IsNullOrEmpty(book.PublicationDate)
            ? "N/A"
            : book.PublicationDate;

        string content =
            book.Title.Bold() + "\n\n" +
            description + "\n\n" +
            "Publisher: ".Gray() + publisher + "\n" +
            "Published: ".Gray() + publicationDate + "\n\n" +
            "Learn more: ".Gray() + displayUrl.Link(urlWithTracking).Cyan();

        terminal.WritePanel(panel => panel
            .Content(content)
            .Border(BorderStyle.Rounded)
            .BorderColor(AnsiColors.Blue)
            .Padding(1, 0));

        terminal.WriteLine();
    }

    private static async Task<List<Book>> FetchBooksFromUrlAsync()
    {
        string response = await HttpClient.GetStringAsync(BooksJsonUrl);
        List<Book>? books = JsonSerializer.Deserialize<List<Book>>(response, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return books ?? [];
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
        Match match = Regex.Match(publicationDate, @"\b(19|20)\d{2}\b");
        if (match.Success && int.TryParse(match.Value, out int extractedYear))
            return extractedYear;

        return 0;
    }

    private sealed class Book
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
