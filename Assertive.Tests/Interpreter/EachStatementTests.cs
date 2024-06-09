using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Tests.Interpreter
{
    public class EachStatementTests: InterpreterTests
    {
        [Fact]
        public async Task EachStatementLoopsList()
        {
            const string program = @"$list = [1, 2, 3];
                                      $sum = 0;
                                      each($x in $list){ $sum = $sum + $x;}
                                      out $sum;";

            await Sut.Execute(program);

            Assert.Equal("6", TestRecorder.Output);
        }
    }
}
