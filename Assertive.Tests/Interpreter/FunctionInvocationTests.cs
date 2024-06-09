namespace Assertive.Tests.Interpreter
{
    public class FunctionInvocationTests : InterpreterTests
    {
        [Fact]
        public async Task UserDefinedFunctionNoParamsNoReturnValue()
        {
            var program = @"def MyFunc {
                                out 'called MyFunction';
                            }
                            
                            MyFunc;";

            await Sut.Execute(program);

            Assert.Equal("called MyFunction", TestRecorder.Output);
        }

        [Fact]
        public async Task UserDefinedFunctionWithParamsNoReturnValue()
        {
            var program = @"def Square ($x) {
                              out $x * $x; 
                            }

                            Square(5);";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

        [Fact]
        public async Task UserDefinedFunctionNoParamsWithReturnValue()
        {
            var program = @"def MyFunc {
                                return 5 * 5;
                            }
                            
                            out MyFunc;";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

        [Fact]
        public async Task UserDefinedFunctionWithParamsWithReturnValue()
        {
            var program = @"def Square ($x) {
                              return $x * $x; 
                            }

                            out Square(5);";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

        [Fact]
        public async Task UserDefinedFunctionWithMultipleParams()
        {
            var program = @"def sum ($x, $y) {
                              return $x + $y; 
                            }

                            out sum(5, 4);";

            await Sut.Execute(program);

            Assert.Equal("9", TestRecorder.Output);
        }

        [Fact]
        public async Task UserDefinedFunctionInvokeMultipleTimes()
        {
            var program = @"def Square ($x) {
                              return $x * $x; 
                            }

                            out Square(3) + Square(4) + Square(5);";

            await Sut.Execute(program);

            Assert.Equal("50", TestRecorder.Output);
        }
    }
}
