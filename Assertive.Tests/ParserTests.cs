namespace Assertive.Tests
{
    public class ParserTests
    {

        [Fact]
        public void ParserReportsSyntaxErrors()
        {
            const string program = "dsfjkhks";
            var parsedDocument = Parser.Parse(program, "c:\\unittestfile.ass");

            Assert.NotEmpty(parsedDocument.SyntaxErrors);
        }

        [Fact]
        public void ParserReportsNoErrorsWhenSyntaxValid()
        {
            const string program = "GET 'http://www.unittest.com';";
            var parsedDocument = Parser.Parse(program, "c:\\unittestfile.ass");

            Assert.Empty(parsedDocument.SyntaxErrors);
        }
    }
}
