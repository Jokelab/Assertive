namespace Assertive.Tests.Interpreter
{
    public class LoopStatementTests : InterpreterTests
    {
        [Fact]
        public async Task LoopStatementSimple()
        {
            const string program = @"
                $x = 0;
                loop from 1 to 10{
                    $x = $x + 1;
                }
                out $x;";

            await Sut.Execute(program);

            Assert.Equal("10", TestRecorder.Output);
        }

        [Fact]
        public async Task LoopStatementWithExpressions()
        {
            const string program = @"
                $x = 0;
                $upper = 5 * 3;
                loop from (3 - 2) to $upper {
                    $x = $x + 1;
                }
                out $x;";

            await Sut.Execute(program);

            Assert.Equal("15", TestRecorder.Output);
        }

        [Fact]
        public async Task LoopStatementWithVariable()
        {
            const string program = @"
                $x = 0;
                loop $i from 1 to 10 {
                    $x = $x + $i;
                }
                out $x;";

            await Sut.Execute(program);

            Assert.Equal("55", TestRecorder.Output);
        }

    }
}
