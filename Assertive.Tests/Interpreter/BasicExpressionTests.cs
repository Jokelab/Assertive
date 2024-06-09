namespace Assertive.Tests.Interpreter
{
    public class BasicExpressionTests: InterpreterTests
    {
        [Fact]
        public async Task VarExpression()
        {
            const string program = "$x = 'hello world'; out $x;";

            await Sut.Execute(program);

            Assert.Equal("hello world", TestRecorder.Output);
        }

        [Fact]
        public async Task IntExpression()
        {
            const string program = "out 1337;";

            await Sut.Execute(program);

            Assert.Equal("1337", TestRecorder.Output);
        }

        [Theory]
        [InlineData("true")]
        [InlineData("false")]
        public async Task BoolExpression(string boolValue)
        {
            var program = $"out {boolValue};";

            await Sut.Execute(program);

            Assert.Equal(boolValue, TestRecorder.Output);
        }

        [Theory]
        [InlineData("\"double quoted string\"","double quoted string")]
        [InlineData("'single quoted string'", "single quoted string")]
        [InlineData("\"Double quoted interpolation: 1 + 1 = {{1 + 1}}\"", "Double quoted interpolation: 1 + 1 = 2")]
        [InlineData("'Single quoted interpolation: 1 + 1 = {{1 + 1}}'", "Single quoted interpolation: 1 + 1 = 2")]
        [InlineData("\"JSON in double quoted string: {'a': '{{1 + 1}}'}\"", "JSON in double quoted string: {'a': '2'}")]
        [InlineData("'JSON in single quoted string: {\"a\": \"{{1 + 1}}\"}'", "JSON in single quoted string: {\"a\": \"2\"}")]
        public async Task StringExpression(string input, string output)
        {
            var program = $"out {input};";

            await Sut.Execute(program);

            Assert.Equal(output, TestRecorder.Output);
        }

        [Fact]
        public async Task ListExpression()
        {
            var program = $"out [1, 2, 3];";

            await Sut.Execute(program);

            Assert.Equal("[1,2,3]", TestRecorder.Output);
        }

        [Fact]
        public async Task DictionaryExpression()
        {
            var program = "out {'a': true, 'b': 2};";

            await Sut.Execute(program);

            Assert.Equal("{a:true,b:2}", TestRecorder.Output);
        }

        [Fact]
        public async Task ListExpressionRecursive()
        {
            var program = $"out [1, 2, 3, [4, 5]];";

            await Sut.Execute(program);

            Assert.Equal("[1,2,3,[4,5]]", TestRecorder.Output);
        }

        [Fact]
        public async Task ParenthesisExpression()
        {
            const string program = "out (5 * 4) + (3 * (3 + 7));";

            await Sut.Execute(program);

            Assert.Equal("50", TestRecorder.Output);
        }

    }
}
