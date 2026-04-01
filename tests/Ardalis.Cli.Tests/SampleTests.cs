namespace Ardalis.Cli.Tests;

public class SampleTests
{
    [Test]
    public async Task TrueIsTrue()
    {
        await Assert.That(true).IsTrue();
    }
}
