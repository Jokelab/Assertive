using Assertive.Models;

namespace Assertive.Tests.Interpreter
{
    public class AnalyseTests : InterpreterTests
    {
        [Fact]
        public void AnalyseReportsSyntaxError()
        {
            const string program = "this makes no sense";

            var result = Sut.Analyse(program, string.Empty);

            Assert.NotEmpty(result.SyntaxErrors);
            Assert.Empty(result.SemanticErrors);
        }

        [Fact]
        public void AnalyseReportsNonExistingFunction()
        {
            const string program = "def test{} nonExisting;";

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.FunctionNotFound));
            Assert.Empty(result.SyntaxErrors);
        }

        [Fact]
        public void AnalyseReportsFunctionOutOfScopeFunction()
        {
            const string program = @"def parent{  
                                        def child {
                                        }                        
                                    } 
                                    child;"; //cannot call child function from outside the parent function
                                        

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.FunctionNotFound));
            Assert.Empty(result.SyntaxErrors);
        }

        [Fact]
        public void AnalyseReportsNonExistingVariable()
        {
            const string program = "$x = $y;";

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.VariableNotFound));
            Assert.Empty(result.SyntaxErrors);
        }

        [Fact]
        public void AnalyseReportsImportedFileNotFound()
        {
            const string program = "import 'nonexisting.ass';";

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.ImportFileNotFound));
            Assert.Empty(result.SyntaxErrors);
        }

        [Fact]
        public void AnalyseReportsImportedFileHasSyntaxErrors()
        {
            const string program = "import 'myfile.ass';";
            FileSystemMock.Setup(x => x.GetFileContent("myfile.ass")).Returns("syntax errors file");
            FileSystemMock.Setup(x => x.FileExists("myfile.ass")).Returns(true);

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.ImportFileSyntaxErrors));
            Assert.Empty(result.SyntaxErrors);
        }

        [Fact]
        public void AnalyseReportsImportedFileDuplicate()
        {
            const string program = @"import 'myfile.ass';
                                     import 'myfile.ass';
                                    ";

            FileSystemMock.Setup(x => x.GetFileContent("myfile.ass")).Returns("");
            FileSystemMock.Setup(x => x.FileExists("myfile.ass")).Returns(true);

            var result = Sut.Analyse(program, string.Empty);

            Assert.Equal(1, result.SemanticErrors.Count(x => x.ErrorCode == ErrorCodes.ImportFileAlreadyImported));
            Assert.Empty(result.SyntaxErrors);
        }

  

    }
}
