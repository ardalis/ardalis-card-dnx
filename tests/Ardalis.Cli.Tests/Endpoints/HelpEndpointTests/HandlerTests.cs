using Ardalis.Cli.Endpoints;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Tests.Endpoints.HelpEndpointTests;

/// <summary>
/// Tests for HelpEndpoint, which gives interactive mode a working "help" command (issue #81).
/// </summary>
public class HandlerTests
{
  [Test]
  public async Task GetCommands_IncludesEveryRoutedEndpoint()
  {
    var names = HelpEndpoint.GetCommands().Select(c => c.Name).ToList();

    await Assert.That(names).Contains("card");
    await Assert.That(names).Contains("version");
    await Assert.That(names).Contains("dotnetconf-score");
    await Assert.That(names).Contains("blog");
    await Assert.That(names).Contains("help");
  }

  [Test]
  public async Task GetCommands_IncludesDescriptions()
  {
    var card = HelpEndpoint.GetCommands().Single(c => c.Name == "card");

    await Assert.That(card.Description).IsEqualTo("Display Ardalis's business card");
  }

  [Test]
  public async Task GetCommands_IncludesAliases()
  {
    var score = HelpEndpoint.GetCommands().Single(c => c.Name == "dotnetconf-score");

    await Assert.That(score.Aliases).Contains("dotnetconf-stats");
  }

  [Test]
  public async Task GetCommands_IsSortedByName()
  {
    var names = HelpEndpoint.GetCommands().Select(c => c.Name).ToList();
    var sorted = names.OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

    await Assert.That(names).IsEquivalentTo(sorted, TUnit.Assertions.Enums.CollectionOrdering.Matching);
  }

  [Test]
  public async Task Render_WritesCommandsAndInteractiveBuiltIns()
  {
    using var terminal = new TestTerminal();

    HelpEndpoint.Render(terminal);

    string output = terminal.Output;
    await Assert.That(output).Contains("card");
    await Assert.That(output).Contains("Display Ardalis's business card");
    await Assert.That(output).Contains("dotnetconf-stats");
    await Assert.That(output).Contains("Alias for dotnetconf-score");
    await Assert.That(output).Contains("exit, quit, q");
    await Assert.That(output).Contains("clear-history");
    await Assert.That(output).Contains("--help");
  }

  [Test]
  [NotInParallel]
  public async Task Main_RoutesHelpCommand_ToCommandList()
  {
    // Interactive mode sends each line through the same generated router as the command line,
    // so routing "help" through Program.Main covers the issue #81 regression.
    // Point telemetry at an unreachable local port so the test sends no analytics events.
    string? originalHost = Environment.GetEnvironmentVariable("POSTHOG_HOST");
    int exitCode;
    try
    {
      Environment.SetEnvironmentVariable("POSTHOG_HOST", "http://127.0.0.1:9/");

      exitCode = await Program.Main(["help"]);
    }
    finally
    {
      Environment.SetEnvironmentVariable("POSTHOG_HOST", originalHost);
    }

    // TUnit captures console output per test.
    string text = TestContext.Current!.GetStandardOutput();
    await Assert.That(exitCode).IsEqualTo(0);
    await Assert.That(text).Contains("Commands:");
    await Assert.That(text).Contains("Display Ardalis's business card");
    await Assert.That(text).DoesNotContain("Unknown command");
  }
}
