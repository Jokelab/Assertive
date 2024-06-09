namespace Assertive.Tests.Interpreter
{
    public class ArithmeticExpressionTests : InterpreterTests
    {
        [Theory]
        [InlineData("1 + 2", "3")]
        [InlineData("5 * 7", "35")]
        [InlineData("7 - 5", "2")]
        [InlineData("7 - 8", "-1")]
        [InlineData("-1 - -2", "1")]
        [InlineData("(-1) - (-2)", "1")]
        [InlineData("-2 + -2", "-4")]
        [InlineData("3 * -1", "-3")]
        [InlineData("8 / 2", "4")]
        [InlineData("1 = 1", "true")]
        [InlineData("1 = 2", "false")]
        [InlineData("1 != 1", "false")]
        [InlineData("1 != 2", "true")]
        [InlineData("2 > 1", "true")]
        [InlineData("2 > 2", "false")]
        [InlineData("2 > 3", "false")]
        [InlineData("2 >= 2", "true")]
        [InlineData("2 >= 3", "false")]
        [InlineData("2 < 3", "true")]
        [InlineData("2 < 2", "false")]
        [InlineData("3 < 2", "false")]
        [InlineData("3 <= 2", "false")]
        [InlineData("2 <= 2", "true")]
        [InlineData("2 <= 3", "true")]
        public async Task ArithmeticBinaryOperators(string expression, string output)
        {
            var program = $"out {expression};";

            await Sut.Execute(program);

            Assert.Equal(output, TestRecorder.Output);
        }
    }
}
