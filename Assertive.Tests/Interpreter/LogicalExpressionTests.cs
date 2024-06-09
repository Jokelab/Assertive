namespace Assertive.Tests.Interpreter
{
    public class LogicalExpressionTests : InterpreterTests
    {
        [Fact]
        public async Task UnaryLogicalExpression()
        {
            const string program = "out not true;";

            await Sut.Execute(program);

            Assert.Equal("false", TestRecorder.Output);
        }

        [Theory]
        [InlineData("true and true", "true")]
        [InlineData("true and false", "false")]
        [InlineData("false and true", "false")]
        [InlineData("false and false", "false")]
        public async Task BinaryLogicalAndExpression(string expression, string output)
        {
            var program = $"out {expression};";

            await Sut.Execute(program);

            Assert.Equal(output, TestRecorder.Output);
        }

        [Theory]
        [InlineData("true or true", "true")]
        [InlineData("true or false", "true")]
        [InlineData("false or true", "true")]
        [InlineData("false or false", "false")]
        public async Task BinaryLogicalOrExpression(string expression, string output)
        {
            var program = $"out {expression};";

            await Sut.Execute(program);

            Assert.Equal(output, TestRecorder.Output);
        }
    }
}
