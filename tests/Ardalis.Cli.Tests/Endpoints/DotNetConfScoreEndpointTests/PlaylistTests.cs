using Ardalis.Cli.Endpoints;
using PlaylistInfo = Ardalis.Cli.Endpoints.DotNetConfScoreEndpoint.PlaylistInfo;

namespace Ardalis.Cli.Tests.Endpoints.DotNetConfScoreEndpointTests;

/// <summary>
/// Tests for the built-in .NET Conf playlists and how they merge with remote playlists.json data.
/// </summary>
public class PlaylistTests
{
    private static PlaylistInfo Remote(int year, string url) => new()
    {
        Name = $".NET Conf {year}",
        Url = url
    };

    [Test]
    public async Task BuiltInPlaylists_Include2026Playlist()
    {
        PlaylistInfo? playlist = DotNetConfScoreEndpoint.Handler.BuiltInPlaylists
            .SingleOrDefault(p => DotNetConfScoreEndpoint.Handler.ExtractYear(p.Name) == 2026);

        await Assert.That(playlist).IsNotNull();
        await Assert.That(playlist!.Url).IsEqualTo("https://www.youtube.com/playlist?list=PLJL9Ln_f6sGM");
        await Assert.That(playlist.HighlightVideos.Select(v => v.Url))
            .Contains("https://www.youtube.com/watch?v=HK3IwBezg0w");
    }

    [Test]
    public async Task MergePlaylists_Adds2026_WhenRemoteDataLacksIt()
    {
        List<PlaylistInfo> remote = [Remote(2025, "remote-2025"), Remote(2024, "remote-2024")];

        List<PlaylistInfo> merged = DotNetConfScoreEndpoint.Handler.MergePlaylists(
            remote, DotNetConfScoreEndpoint.Handler.BuiltInPlaylists);

        await Assert.That(merged.Select(p => DotNetConfScoreEndpoint.Handler.ExtractYear(p.Name)))
            .Contains(2026);
    }

    [Test]
    public async Task MergePlaylists_PrefersRemoteEntry_WhenYearExistsInBoth()
    {
        List<PlaylistInfo> remote = [Remote(2026, "remote-2026")];

        List<PlaylistInfo> merged = DotNetConfScoreEndpoint.Handler.MergePlaylists(
            remote, DotNetConfScoreEndpoint.Handler.BuiltInPlaylists);

        List<PlaylistInfo> entries2026 = merged
            .Where(p => DotNetConfScoreEndpoint.Handler.ExtractYear(p.Name) == 2026)
            .ToList();
        await Assert.That(entries2026.Count).IsEqualTo(1);
        await Assert.That(entries2026[0].Url).IsEqualTo("remote-2026");
    }

    [Test]
    public async Task MergePlaylists_OrdersNewestYearFirst()
    {
        List<PlaylistInfo> remote = [Remote(2021, "remote-2021")];

        List<int?> years = DotNetConfScoreEndpoint.Handler.MergePlaylists(
                remote, DotNetConfScoreEndpoint.Handler.BuiltInPlaylists)
            .Select(p => DotNetConfScoreEndpoint.Handler.ExtractYear(p.Name))
            .ToList();

        await Assert.That(string.Join(",", years)).IsEqualTo("2026,2025,2024,2023,2022,2021");
    }

    [Test]
    public async Task MergePlaylists_ReturnsBuiltIns_WhenRemoteIsEmpty()
    {
        List<PlaylistInfo> merged = DotNetConfScoreEndpoint.Handler.MergePlaylists(
            [], DotNetConfScoreEndpoint.Handler.BuiltInPlaylists);

        await Assert.That(merged.Count).IsEqualTo(DotNetConfScoreEndpoint.Handler.BuiltInPlaylists.Count);
    }
}
