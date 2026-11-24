using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using TimeWarp.Terminal;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// Lists the available commands. Provides a plain <c>help</c> command so that typing
/// <c>help</c> works in interactive (REPL) mode, where only <c>--help</c> was built in.
/// </summary>
[NuruRoute("help", Description = "List available commands")]
public sealed class HelpEndpoint : IQuery<Unit>
{
  /// <summary>
  /// A command discovered from a <see cref="NuruRouteAttribute"/> on an endpoint type.
  /// </summary>
  public sealed record CommandInfo(string Name, string Description, IReadOnlyList<string> Aliases);

  private static readonly Lazy<IReadOnlyList<CommandInfo>> Commands = new(DiscoverCommands);

  /// <summary>
  /// Gets all commands declared with <see cref="NuruRouteAttribute"/> in this assembly,
  /// sorted by name.
  /// </summary>
  public static IReadOnlyList<CommandInfo> GetCommands() => Commands.Value;

  private static IReadOnlyList<CommandInfo> DiscoverCommands() =>
    [.. GetLoadableTypes(typeof(HelpEndpoint).Assembly)
      .Select(type => (Route: type.GetCustomAttribute<NuruRouteAttribute>(), Alias: type.GetCustomAttribute<NuruRouteAliasAttribute>()))
      .Where(x => x.Route is not null && !string.IsNullOrWhiteSpace(x.Route.Pattern))
      .Select(x => new CommandInfo(
        x.Route!.Pattern,
        x.Route.Description ?? string.Empty,
        x.Alias?.Aliases ?? []))
      .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)];

  private static Type[] GetLoadableTypes(Assembly assembly)
  {
    try
    {
      return assembly.GetTypes();
    }
    catch (ReflectionTypeLoadException ex)
    {
      // Still list the commands whose types did load.
      return [.. ex.Types.OfType<Type>()];
    }
  }

  /// <summary>
  /// Writes the command list, plus the interactive-mode built-ins, to the terminal.
  /// </summary>
  public static void Render(ITerminal terminal)
  {
    IReadOnlyList<CommandInfo> commands = GetCommands();

    // Keep command names whole on narrow terminals; only the description may be truncated.
    int nameWidth = commands
      .SelectMany(c => c.Aliases.Prepend(c.Name))
      .Select(name => name.Length)
      .DefaultIfEmpty(0)
      .Max();

    terminal.WriteLine("Commands:".Cyan().Bold());
    terminal.WriteTable(table =>
    {
      table
        .AddColumn(new TableColumn("Command") { MinWidth = nameWidth })
        .AddColumn("Description")
        .HideHeaders();
      foreach (CommandInfo command in commands)
      {
        table.AddRow(command.Name, command.Description);
        foreach (string alias in command.Aliases)
        {
          table.AddRow(alias, $"Alias for {command.Name}");
        }
      }
    });

    terminal.WriteLine("Interactive mode:".Cyan().Bold());
    terminal.WriteTable(table => table
      .AddColumn("Command")
      .AddColumn("Description")
      .HideHeaders()
      .AddRow("exit, quit, q", "Leave interactive mode")
      .AddRow("clear, cls", "Clear the screen")
      .AddRow("history", "Show command history")
      .AddRow("clear-history", "Clear command history"));

    terminal.WriteLine("Run '<command> --help' for details on a command's options.");
  }

  public sealed class Handler : IQueryHandler<HelpEndpoint, Unit>
  {
    public ValueTask<Unit> Handle(HelpEndpoint query, CancellationToken ct)
    {
      Render(TimeWarpTerminal.Default);
      return default;
    }
  }
}
