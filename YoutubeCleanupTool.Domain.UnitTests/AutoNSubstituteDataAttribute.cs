﻿using AutoFixture;
using AutoFixture.AutoNSubstitute;
using AutoFixture.Xunit2;

namespace YouTubeCleanupTool.Domain.UnitTests
{
    public class AutoNSubstituteDataAttribute : AutoDataAttribute
    {
        public AutoNSubstituteDataAttribute()
            : base(() => new Fixture().Customize(new CompositeCustomization(
                new AutoNSubstituteCustomization(),
                // TODO: Need to redesign models etc so we don't have to do this. It's happening because
                // VideoData -> List<PlaylistData> -> VideoData -> etc
                new DontThrowRecursionErrorsCustomization()
                )))
        {
        }
    }
}
