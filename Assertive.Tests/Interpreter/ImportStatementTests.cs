namespace Assertive.Tests.Interpreter
{
    public class ImportStatementTests: InterpreterTests
    {
        [Fact]
        public async Task CanCallFunctionOfImportedFile()
        {
            var initialProgram = @"import 'myfile.ass';
                                   externalFunction();
                                  ";
            
            var myfile = "def externalFunction(){ out 'hello from external function'; }";
            FileSystemMock.Setup(x => x.GetFileContent("myfile.ass")).Returns(myfile);
            FileSystemMock.Setup(x => x.FileExists("myfile.ass")).Returns(true);


            await Sut.Execute(initialProgram);

            Assert.Equal("hello from external function", TestRecorder.Output);

        }

        [Fact]
        public async Task CanCallFunctionOfIndirectlyImportedFile()
        {
            var initialProgram = @"import 'myfile1.ass';
                                   externalFunctionInMyFile2();
                                  ";

            var myfile1 = "import 'myfile2.ass';";
            FileSystemMock.Setup(x => x.GetFileContent("myfile1.ass")).Returns(myfile1);
            var myfile2 = "def externalFunctionInMyFile2(){ out 'hello from myfile2'; }";
            FileSystemMock.Setup(x => x.GetFileContent("myfile2.ass")).Returns(myfile2);

            FileSystemMock.Setup(x => x.FileExists("myfile1.ass")).Returns(true);
            FileSystemMock.Setup(x => x.FileExists("myfile2.ass")).Returns(true);


            await Sut.Execute(initialProgram);

            Assert.Equal("hello from myfile2", TestRecorder.Output);

        }

        [Fact]
        public async Task CanUseVariableOfImportedFile()
        {
            var initialProgram = @"import 'myfile.ass';
                                   out $externalVar;
                                  ";

            var myfile = "$externalVar = 'external var content';";
            FileSystemMock.Setup(x => x.FileExists("myfile.ass")).Returns(true);
            FileSystemMock.Setup(x => x.GetFileContent("myfile.ass")).Returns(myfile);


            await Sut.Execute(initialProgram);

            Assert.Equal("external var content", TestRecorder.Output);

        }

    }
}
