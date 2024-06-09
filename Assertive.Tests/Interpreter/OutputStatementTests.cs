namespace Assertive.Tests.Interpreter
{
    public class OutputStatementTests: InterpreterTests
    {
        [Fact]
        public async Task OutputStaticString()
        {
            const string program = "out 'Hello world';";

            await Sut.Execute(program);

            Assert.Equal("Hello world", TestRecorder.Output);
        }

    }
}
