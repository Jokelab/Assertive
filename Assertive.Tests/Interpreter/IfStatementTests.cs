namespace Assertive.Tests.Interpreter
{
    public class IfStatementTests : InterpreterTests
    {
        [Fact]
        public async Task IfStatementTrue()
        {
            const string program = @"if (true){ out 'condition true'; }";

            await Sut.Execute(program);

            Assert.Equal("condition true", TestRecorder.Output);
        }

        [Fact]
        public async Task IfStatementFalse()
        {
            const string program = @"if (false){ out 'this should not be in output'; }";

            await Sut.Execute(program);

            Assert.Equal("", TestRecorder.Output);
        }

        [Fact]
        public async Task IfStatementTrueWithElseBranch()
        {
            const string program = @"if (true) {out 'condition true';} else { out 'condition false'; }";

            await Sut.Execute(program);

            Assert.Equal("condition true", TestRecorder.Output);
        }

        [Fact]
        public async Task IfStatementFalseWithElseBranch()
        {
            const string program = @"if (false) {out 'condition true';} else { out 'condition false'; }";

            await Sut.Execute(program);

            Assert.Equal("condition false", TestRecorder.Output);
        }
    }
}
