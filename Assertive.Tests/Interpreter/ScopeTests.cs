namespace Assertive.Tests.Interpreter
{
    public class ScopeTests : InterpreterTests
    {
        [Fact]
        public async Task GlobalVariablesAreVisibleInFunction()
        {
            const string program =
                @"
                $x = 'hello';
                def func{
                    out $x;
                }
                func;
                ";

            await Sut.Execute(program);

            Assert.Equal("hello", TestRecorder.Output);
        }

        [Fact]
        public async Task FunctionVariablesAreNotVisibleOutsideFunction()
        {
            const string program =
                @"
                $x = 'foo';
                def func{
                    $x = 'bar';
                    out $x;
                }
                func;
                ";

            await Sut.Execute(program);

            Assert.Equal("bar", TestRecorder.Output);
        }

        [Fact]
        public async Task ParamBindingShouldNotModifyParentScopeVariable()
        {
            const string program =
                @"
                $x = 'parent scope value of x';
                def pow($x){
                    return $x * $x;
                }
                pow(5);
                out $x;
                ";

            await Sut.Execute(program);

            Assert.Equal("parent scope value of x", TestRecorder.Output);
        }

        [Fact]
        public async Task FunctionCanModifyParentScopeVariable()
        {
            const string program =
                @"
                $x = 'parent scope value of x';
                def pow($y){
                    $x = 'modified value';
                    return $y * $y;
                }
                pow(5);
                out $x;
                ";

            await Sut.Execute(program);

            Assert.Equal("modified value", TestRecorder.Output);
        }

        [Fact]
        public async Task FunctionCanBeInvokedBeforeDeclaration()
        {
            const string program =
                @"
                out pow(5);
                def pow($x){
                    return $x * $x;
                }
                ";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

        [Fact]
        public async Task FunctionsCanBeNested()
        {
            const string program =
                @"
               
                def parent($y){
                    
                    return child($y);
       
                    def child($x){
                        return $x * $x;
                    }
                    
                }
                out parent(5);
                ";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

        [Fact]
        public async Task FunctionsCanCallParentScopeFunctions()
        {
            const string program =
                @"
               
                def parent1($y){
                    
                    return child($y);
       
                    def child($x){
                        return parent2($x);
                    }
                    
                }
                def parent2($x) {return $x * $x;}
                out parent1(5);
                ";

            await Sut.Execute(program);

            Assert.Equal("25", TestRecorder.Output);
        }

    }
}
