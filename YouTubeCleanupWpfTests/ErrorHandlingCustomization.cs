using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

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
