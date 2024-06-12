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
    }
}
