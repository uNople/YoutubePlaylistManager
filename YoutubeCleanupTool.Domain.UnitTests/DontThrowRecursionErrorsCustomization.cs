using System.Linq;
using AutoFixture;

namespace YouTubeCleanupTool.Domain.UnitTests
{
    public class DontThrowRecursionErrorsCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }
    }
}
