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
    await Assert.That(output).Contains("--help");
  }

  [Test]
  public async Task Handle_DoesNotThrow()
  {
    var handler = new HelpEndpoint.Handler();

    await handler.Handle(new HelpEndpoint(), CancellationToken.None);
  }
}
