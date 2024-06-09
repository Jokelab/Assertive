namespace Assertive.Tests.Interpreter
{
    public class WhileStatementTests : InterpreterTests
    {
        [Fact]
        public async Task WhileStatement()
        {
            const string program = @"
                $x = 1;
                while ($x < 10){ $x = $x + 1; }
                out $x;";

            await Sut.Execute(program);

            Assert.Equal("10", TestRecorder.Output);
        }

        [Fact]
        public async Task WhileStatementSkippedWhenConditionIsFalse()
        {
            const string program = @"
                $x = 1;
                while ($x > 2){ $x = $x + 1; }
                out $x;";

            await Sut.Execute(program);

            Assert.Equal("1", TestRecorder.Output);
        }

    }
}
