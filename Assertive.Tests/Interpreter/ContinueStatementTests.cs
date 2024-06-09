namespace Assertive.Tests.Interpreter
{
    public class ContinueStatementTests: InterpreterTests
    {

        [Fact]
        public async Task ContinueStatementGoesToNextIterationOfWhileLoop()
        {
            const string program = @"
                                      $x = 0;
                                      $sum = 0;
                                      while($x < 10){ 
                                        $x = $x + 1;
                                        if ($x = 2){ continue; }
                                        $sum = $sum + 1;
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("9", TestRecorder.Output);
        }


        [Fact]
        public async Task ContinueStatementGoesToNextIterationInEachLoop()
        {
            const string program = @"$list = [1, 2, 3];
                                      $sum = 0;
                                      each($x in $list){ 
                                        
                                        if ($x = 2){ continue; }
                                        $sum = $sum + $x;
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("4", TestRecorder.Output);
        }

        [Fact]
        public async Task ContinueStatementGoesToNextIterationInLoop()
        {
            const string program = @"
                                      $sum = 0;
                                      loop $x from 1 to 5{ 
                                        
                                        if ($x = 2){ continue; }
                                        $sum = $sum + $x;
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("13", TestRecorder.Output);
        }

        [Fact]
        public async Task ContinueStatementInNestedLoops()
        {
            const string program = @"
                                      $sum = 0;
                                      loop $x from 1 to 3{ 
                                        
                                        if ($x > 2) { continue;}
                                        $sum = $sum + 1;
                                    
                                        loop $y from 1 to 3{
                                            if ($y > 2) { continue;}
                                            $sum = $sum + 1;
                                           
                                        }
                                            
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("6", TestRecorder.Output);
        }


    }
}
