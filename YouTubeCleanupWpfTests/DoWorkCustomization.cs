using AutoFixture;
using YouTubeCleanup.Ui;

namespace YouTubeCleanupWpf.UnitTests
{
    public class DoWorkCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            fixture.Register<IDoWorkOnUi>(fixture.Create<DoWorkOnUi>);
        }
    }
}