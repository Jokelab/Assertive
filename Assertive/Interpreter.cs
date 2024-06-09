using Antlr4.Runtime;
using Assertive.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Assertive
{
    public class Interpreter
    {
        private readonly ProgramVisitor _programVisitor;
        private readonly ErrorListener _errorListener;
        private readonly IFileSystemService _fileSystemService;
        private readonly ILogger<Interpreter> _logger;
        private List<string> _importedFiles = [];

        public Interpreter(ProgramVisitor programVisitor, ErrorListener errorListener, IFileSystemService fileSystemService, ILogger<Interpreter> logger)
        {
            _programVisitor = programVisitor;
            _errorListener = errorListener;
            _fileSystemService = fileSystemService;
            _logger = logger;
        }

        public async Task ExecuteFile(string path)
        {
            _importedFiles.Clear();
            _importedFiles.Add(path);
            await Execute(_fileSystemService.GetFileContent(path)).ConfigureAwait(false);
        }

        public List<SyntaxErrorModel> GetSyntaxErrors()
        {
            return _errorListener.SyntaxErrors;
        }

        private string GetCurrentPath()
        {
            return _importedFiles.Count > 0 ? _importedFiles[0] : Assembly.GetExecutingAssembly().Location;
        }

        public async Task Execute(string program)
        {

            var startFileParser = CreateParser(program);
            AssertiveParser.ProgramContext startContext = startFileParser.program();
            if (_errorListener.SyntaxErrors.Count > 0)
            {
                return;
            }
            var currentPath = GetCurrentPath();
            var documents = GetImportedDocuments(startContext, currentPath);
            documents.Add(new ParsedDocument(startContext, currentPath));

            foreach (var document in documents)
            {
                _programVisitor.FilePath = document.Path;
                //execute main visitor class
                await _programVisitor.Visit(document.Context).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Recursively import files
        /// </summary>
        /// <param name="context"></param>
        /// <param name="programBuilder"></param>
        /// <returns></returns>
        private List<ParsedDocument> GetImportedDocuments(AssertiveParser.ProgramContext context, string currentPath)
        {
            var importedPrograms = new List<ParsedDocument>();
            if (context.importStatements().ChildCount == 0)
            {
                return importedPrograms;
            }
            var imports = context.importStatements().children;
            foreach (var importStatement in imports)
            {
                var path = _fileSystemService.CalculateRelativePath(currentPath, importStatement.GetChild(1).GetText()).Replace("\"", "").Replace("'", "");
                if (_importedFiles.Contains(path))
                {
                    _logger.LogError($"Already imported file {path}. The same file cannot be imported multiple times.");
                    continue;
                }
                _importedFiles.Add(path);
                var fileContent = _fileSystemService.GetFileContent(path);

                var parser = CreateParser(fileContent);
                var importContext = parser.program();
                if (_errorListener.SyntaxErrors.Count > 0)
                {
                    _logger.LogError($"Syntax error(s) in imported file {path}");
                    break;
                }
                var childImports = GetImportedDocuments(importContext, path);
                importedPrograms.AddRange(childImports);

                importedPrograms.Add(new ParsedDocument(importContext, path));
            }
            return importedPrograms;
        }

        public AssertiveParser CreateParser(string program)
        {
            var inputStream = new AntlrInputStream(program);
            var lexer = new AssertiveLexer(inputStream);
            var commonTokenStream = new CommonTokenStream(lexer);
            var parser = new AssertiveParser(commonTokenStream);
            parser.RemoveErrorListeners();
            parser.AddErrorListener(_errorListener);
            return parser;
        }

        private class ParsedDocument
        {
            public ParsedDocument(AssertiveParser.ProgramContext context, string path)
            {
                Context = context;
                Path = path;
            }
            public AssertiveParser.ProgramContext Context { get; }
            public string Path { get; }
        }
    }
}
