using AutoFixture;
using AutoMapper;

namespace YouTubeCleanupWpf.UnitTests
{
    public class AutoMapperCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var myProfile = new WpfYouTubeMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));

            var mapper = new Mapper(configuration);
            fixture.Register<IMapper>(() => mapper);
        }
    }
}
