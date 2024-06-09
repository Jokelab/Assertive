using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assertive.Tests.Interpreter
{
    public class AssignmentStatementTests: InterpreterTests
    {
        [Fact]
        public async Task VariableAssignment()
        {
            var program = $"$x = 1; out $x;";

            await Sut.Execute(program);

            Assert.Equal("1", TestRecorder.Output);
        }

        [Fact]
        public async Task VariableAssignmentChangeType()
        {
            var program = @$"$x = 123; 
                             $x = 'hello world'; 
                             out $x;";

            await Sut.Execute(program);

            Assert.Equal("hello world", TestRecorder.Output);
        }

    }
}
