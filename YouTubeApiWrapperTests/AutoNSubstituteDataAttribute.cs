using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace YouTubeApiWrapperTests
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new CompositeCustomization(
                new AutoNSubstituteCustomization() { ConfigureMembers = false }
            )))
        {
        }
    }
}
