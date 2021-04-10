using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;
using UnitTests.Common;

namespace YouTubeCleanupWpf.UnitTests
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new CompositeCustomization(
                new AutoNSubstituteCustomization() { ConfigureMembers = false },
                // TODO: Need to redesign models etc so we don't have to do this. It's happening because
                // VideoData -> List<PlaylistData> -> VideoData -> etc
                new DontThrowRecursionErrorsCustomization(),
                new AutoMapperCustomization(),
                new ErrorHandlingCustomization(),
                new DoWorkCustomization()
            )))
        {
        }
    }
}
