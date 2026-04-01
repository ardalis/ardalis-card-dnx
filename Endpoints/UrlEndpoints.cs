using System.Threading;
using System.Threading.Tasks;
using TimeWarp.Nuru;
using TimeWarp.Terminal;
using static Ardalis.Cli.Urls;
using static Ardalis.Helpers.UrlHelper;

namespace Ardalis.Cli.Endpoints;

/// <summary>
/// URL opener endpoints for opening various Ardalis resources in the browser.
/// </summary>
/// 
[NuruRoute("blog", Description = "Open Ardalis's blog")]
public sealed class BlogEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<BlogEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(BlogEndpoint cmd, CancellationToken ct)
        {
            Open(Blog);
            return default;
        }
    }
}

[NuruRoute("bluesky", Description = "Open Ardalis's Bluesky profile")]
public sealed class BlueskyEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<BlueskyEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(BlueskyEndpoint cmd, CancellationToken ct)
        {
            Open(BlueSky);
            return default;
        }
    }
}

[NuruRoute("contact", Description = "Open Ardalis's contact page")]
public sealed class ContactEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<ContactEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(ContactEndpoint cmd, CancellationToken ct)
        {
            Open(Contact);
            return default;
        }
    }
}

[NuruRoute("dometrain", Description = "Open Ardalis's Dometrain Author profile")]
public sealed class DometrainEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<DometrainEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(DometrainEndpoint cmd, CancellationToken ct)
        {
            Open(Dometrain);
            return default;
        }
    }
}

[NuruRoute("github", Description = "Open Ardalis's GitHub profile")]
public sealed class GithubEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<GithubEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(GithubEndpoint cmd, CancellationToken ct)
        {
            Open(GitHub);
            return default;
        }
    }
}

[NuruRoute("linkedin", Description = "Open Ardalis's LinkedIn profile")]
public sealed class LinkedinEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<LinkedinEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(LinkedinEndpoint cmd, CancellationToken ct)
        {
            Open(LinkedIn);
            return default;
        }
    }
}

[NuruRoute("nimblepros", Description = "Open NimblePros website")]
public sealed class NimbleprosEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<NimbleprosEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(NimbleprosEndpoint cmd, CancellationToken ct)
        {
            Open(NimblePros);
            return default;
        }
    }
}

[NuruRoute("nuget", Description = "Open Ardalis's NuGet profile")]
public sealed class NugetEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<NugetEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(NugetEndpoint cmd, CancellationToken ct)
        {
            Open(NuGet);
            return default;
        }
    }
}

[NuruRoute("pluralsight", Description = "Open Ardalis's Pluralsight profile")]
public sealed class PluralsightEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<PluralsightEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(PluralsightEndpoint cmd, CancellationToken ct)
        {
            Open(Pluralsight);
            return default;
        }
    }
}

[NuruRoute("speaker", Description = "Open Ardalis's Sessionize speaker profile")]
public sealed class SpeakerEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<SpeakerEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(SpeakerEndpoint cmd, CancellationToken ct)
        {
            Open(Speaker);
            return default;
        }
    }
}

[NuruRoute("subscribe", Description = "Open Ardalis's newsletter subscription page")]
public sealed class SubscribeEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<SubscribeEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(SubscribeEndpoint cmd, CancellationToken ct)
        {
            Open(Subscribe);
            return default;
        }
    }
}

[NuruRoute("youtube", Description = "Open Ardalis's YouTube channel")]
public sealed class YoutubeEndpoint : ICommand<Unit>
{
    public sealed class Handler : ICommandHandler<YoutubeEndpoint, Unit>
    {
        public ValueTask<Unit> Handle(YoutubeEndpoint cmd, CancellationToken ct)
        {
            Open(YouTube);
            return default;
        }
    }
}
