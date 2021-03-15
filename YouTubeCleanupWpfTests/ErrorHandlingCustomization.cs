using System;
using AutoFixture;
using NSubstitute;

namespace YouTubeCleanupWpf.UnitTests
{
    public class ErrorHandlingCustomization : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var errorHandler = fixture.Freeze<IErrorHandler>();
            errorHandler.When(x => x.HandleError(Arg.Any<Exception>())).Do(x => throw x.Arg<Exception>());
        }
    }
}
