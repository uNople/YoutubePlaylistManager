using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using UnitTests.Common;

namespace YouTubeCleanupTool.Domain.UnitTests;

public class AutoNSubstituteDataAttribute() : AutoDataAttribute(() => new Fixture().Customize(
    new CompositeCustomization(
        new AutoNSubstituteCustomization() { ConfigureMembers = false }, new DontThrowRecursionErrorsCustomization()
    )))
{
    // TODO: Need to redesign models etc so we don't have to do this. It's happening because
    // VideoData -> List<PlaylistData> -> VideoData -> etc
}