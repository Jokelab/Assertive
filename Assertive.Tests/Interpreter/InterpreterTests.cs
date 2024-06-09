using Assertive.Extensions;
using Assertive.Models;
using Assertive.Requests;
using Assertive.Requests.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace Assertive.Tests.Interpreter
{
    public abstract class InterpreterTests
    {
        protected Assertive.Interpreter Sut { get; private set; }
        protected TestOutputRecorder TestRecorder { get; private set; }
        protected Mock<IRequestDispatcher> RequestDispatcherMock { get; private set; } = new();
        protected Mock<IFileSystemService> FileSystemMock { get; private set; } = new();

        protected InterpreterTests()
        {
            TestRecorder = new TestOutputRecorder();
            var serviceProvider = new ServiceCollection()
                .AddAssertive(opt => {
                    opt.AddOutputWriter(TestRecorder);
                    opt.ReplaceRequestDispatcher(RequestDispatcherMock.Object);
                })
                .Replace(new ServiceDescriptor(typeof(IFileSystemService), FileSystemMock.Object))
                .BuildServiceProvider();

            Sut = serviceProvider.GetService<Assertive.Interpreter>()!;

            FileSystemMock.Setup(x => x.CalculateRelativePath(It.IsAny<string>(), It.IsAny<string>())).Returns((string current, string relative) => { return relative; });
            FileSystemMock.Setup(x => x.GetFileStream(It.IsAny<string>())).Returns(new MemoryStream());
            RequestDispatcherMock.Setup(x => x.CreateRequest(It.IsAny<HttpRequestMessage>())).Returns((HttpRequestMessage msg) => new HttpRequest() { Request = msg, Response = new HttpResponseMessage() });
        }

    }
}