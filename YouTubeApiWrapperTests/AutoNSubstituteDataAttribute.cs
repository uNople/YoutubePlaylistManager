using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace YouTubeApiWrapperTests;

public class AutoNSubstituteDataAttribute() : AutoDataAttribute(() => new Fixture().Customize(
    new CompositeCustomization(
        new AutoNSubstituteCustomization() { ConfigureMembers = false }
    )));