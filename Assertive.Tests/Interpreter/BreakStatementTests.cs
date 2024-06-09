using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Tests.Interpreter
{
    public class BreakStatementTests: InterpreterTests
    {

        [Fact]
        public async Task BreakStatementBreaksWhileLoop()
        {
            const string program = @"
                                      $x = 0;
                                      while($x < 10){ 
                                        $x = $x + 1;
                                        if ($x = 2){ break; }
                                      }
                                      out $x;";

            await Sut.Execute(program);

            Assert.Equal("2", TestRecorder.Output);
        }

        [Fact]
        public async Task BreakStatementBreaksEachLoop()
        {
            const string program = @"$list = [1, 2, 3];
                                      $sum = 0;
                                      each($x in $list){ 
                                        $sum = $sum + $x;
                                        if ($x = 2){ break; }
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("3", TestRecorder.Output);
        }

        [Fact]
        public async Task BreakStatementBreaksLoop()
        {
            const string program = @"$list = [1, 2, 3];
                                      $sum = 0;
                                      loop $x from 1 to 5{ 
                                        $sum = $sum + $x;
                                        if ($x = 2){ break; }
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("3", TestRecorder.Output);
        }

        [Fact]
        public async Task BreakStatementSkipsLoop()
        {
            const string program = @"$list = [1, 2, 3];
                                      $sum = 0;
                                      loop $x from 1 to 5
                                      { 
                                        break;
                                        $sum = $sum + $x;
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("0", TestRecorder.Output);
        }

        [Fact]
        public async Task BreakStatementBreaksProgram()
        {
            const string program = @"out 'hello';
                                     break;
                                     out 'world';";

            await Sut.Execute(program);

            Assert.Equal("hello", TestRecorder.Output);
        }

        [Fact]
        public async Task BreakStatementInNestedLoops()
        {
            const string program = @"
                                      $sum = 0;
                                      loop $x from 1 to 5{ 
                                        
                                    
                                        while($x < 10){
                                            $sum = $sum + $x;                                            
                                            if ($x >= 1){ break; }
                                            
                                        }
                                        
                                        $sum = $sum + $x;
                                        if ($x = 2) { break; }
                                            
                                      }
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("6", TestRecorder.Output);
        }

    }
}
