using System.Reflection;
using Ardalis.Api;

namespace Ardalis.Cli.Tests;

/// <summary>
/// Tests for ArdalisApiClient.ExtractPlaylistId — a private static method that normalises
/// YouTube playlist identifiers. It is invoked by GetPlaylistStatsAsync and must handle
/// plain IDs, full playlist URLs, and URLs that embed the list ID alongside other params.
/// Tested via reflection because the extraction logic is independently verifiable.
/// </summary>
public class ArdalisApiClientExtractPlaylistIdTests
{
    private static readonly MethodInfo ExtractPlaylistId =
        typeof(ArdalisApiClient)
            .GetMethod("ExtractPlaylistId", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new MissingMethodException(nameof(ArdalisApiClient), "ExtractPlaylistId");

    private static string Invoke(string input) =>
        (string)ExtractPlaylistId.Invoke(null, [input])!;

    [Test]
    public async Task ReturnsIdUnchanged_WhenInputIsAlreadyAPlainId()
    {
        var result = Invoke("PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1");

        await Assert.That(result).IsEqualTo("PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1");
    }

    [Test]
    public async Task TrimsSurroundingWhitespace_WhenInputIsPlainId()
    {
        var result = Invoke("  PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1  ");

        await Assert.That(result).IsEqualTo("PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1");
    }

    [Test]
    public async Task ExtractsListParam_WhenInputIsFullPlaylistUrl()
    {
        var result = Invoke("https://www.youtube.com/playlist?list=PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1");

        await Assert.That(result).IsEqualTo("PLrZbkNpNVN2eHOCkKR0VU8JXCrPLVAMV1");
    }

    [Test]
    public async Task ExtractsListParam_WhenListIsNotFirstQueryParameter()
    {
        var result = Invoke("https://www.youtube.com/watch?v=someVideoId&list=PLsomeid123");

        await Assert.That(result).IsEqualTo("PLsomeid123");
    }

    [Test]
    public async Task ExtractsListParam_WhenAdditionalParamsFollowList()
    {
        var result = Invoke("https://www.youtube.com/playlist?list=PLsomeid123&index=5&t=10s");

        await Assert.That(result).IsEqualTo("PLsomeid123");
    }

    [Test]
    public async Task ReturnsTrimmedOriginal_WhenUrlContainsNoListParam()
    {
        var result = Invoke("https://www.youtube.com/watch?v=someVideoId");

        await Assert.That(result).IsEqualTo("https://www.youtube.com/watch?v=someVideoId");
    }
}
