using Assertive.Exceptions;
using Assertive.Models;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Assertive
{
    public class Interpreter
    {
        private readonly ProgramVisitor _programVisitor;
        private readonly AnalyserVisitor _analyserVisitor;
        private readonly IFileSystemService _fileSystemService;
        private List<string> _importedFiles = [];

        public Interpreter(ProgramVisitor programVisitor, AnalyserVisitor analyserVisitor, IFileSystemService fileSystemService)
        {
            _programVisitor = programVisitor;
            _analyserVisitor = analyserVisitor;
            _fileSystemService = fileSystemService;
        }

        public async Task<InterpretationResult> ExecuteFile(string path)
        {
            _importedFiles.Clear();
            _importedFiles.Add(path);
            return await Execute(_fileSystemService.GetFileContent(path)).ConfigureAwait(false);
        }

        private string GetCurrentPath()
        {
            return _importedFiles.Count > 0 ? _importedFiles[0] : Assembly.GetExecutingAssembly().Location;
        }

        public async Task<InterpretationResult> Execute(string program)
        {
            var currentPath = GetCurrentPath();
            var result = new InterpretationResult();
            var parsedDocument = Parser.Parse(program, currentPath);
            if (parsedDocument.SyntaxErrors.Count > 0)
            {
                result.SyntaxErrors.AddRange(parsedDocument.SyntaxErrors);
                return result;
            }
            var documents = GetImportedDocuments(parsedDocument.Context, currentPath, result);
            documents.Add(parsedDocument);

            foreach (var document in documents)
            {
                try
                {
                    _programVisitor.FilePath = document.Path;
                    //execute main visitor class
                    await _programVisitor.Visit(document.Context).ConfigureAwait(false);
                }
                catch (InterpretationException interpretationEx)
                {
                    result.SemanticErrors.Add(new SemanticErrorModel() { Context = interpretationEx.Context, FilePath = document.Path, Message = interpretationEx.Message });
                }
            }
            return result;
        }

        /// <summary>
        /// Analyses the program for syntax and semantic errors, without actually executing requests
        /// </summary>
        /// <param name="program"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public InterpretationResult Analyse(string program, string path)
        {
            _importedFiles.Clear();
            _importedFiles.Add(path);
            var currentPath = GetCurrentPath();
            var result = new InterpretationResult();
            var parsedDocument = Parser.Parse(program, currentPath);
            if (parsedDocument.SyntaxErrors.Count > 0)
            {
                result.SyntaxErrors.AddRange(parsedDocument.SyntaxErrors);
                return result;
            }
            var documents = GetImportedDocuments(parsedDocument.Context, currentPath, result);
            documents.Add(parsedDocument);

            foreach (var document in documents)
            {
                _analyserVisitor.FilePath = document.Path;
                _analyserVisitor.Visit(document.Context);
                result.SemanticErrors.AddRange(_analyserVisitor.SemanticErrors);
            }
            return result;
        }

        /// <summary>
        /// Recursively import files
        /// </summary>
        /// <param name="context"></param>
        /// <param name="programBuilder"></param>
        /// <returns></returns>
        private List<ParsedDocument> GetImportedDocuments(AssertiveParser.ProgramContext context, string currentPath, InterpretationResult interpretationResult)
        {
            var importedPrograms = new List<ParsedDocument>();
            if (context.importStatements().ChildCount == 0)
            {
                return importedPrograms;
            }
            var imports = context.importStatements().children;
            for (var i = 0; i < imports.Count; i++)
            {
                var importStatement = context.importStatements().importStatement(i);
                var importFileName = importStatement.GetChild(1).GetText();
                var path = _fileSystemService.CalculateRelativePath(currentPath, importFileName).Replace("\"", "").Replace("'", "");
                if (_importedFiles.Contains(path))
                {
                    interpretationResult.SemanticErrors.Add(new SemanticErrorModel() { Context = importStatement, FilePath = path, Message = $"Already imported file {importFileName} in this file or one of its imports. The same file cannot be imported multiple times." });
                    continue;
                }
                _importedFiles.Add(path);
                var fileContent = _fileSystemService.GetFileContent(path);

                var parsedDocument = Parser.Parse(fileContent, path);
                if (parsedDocument.SyntaxErrors.Count > 0)
                {
                    interpretationResult.SemanticErrors.Add(new SemanticErrorModel() { Context = importStatement, FilePath = path, Message = $"Syntax errors found in imported file {importFileName}" });
                    break;
                }
                var childImports = GetImportedDocuments(parsedDocument.Context, path, interpretationResult);
                importedPrograms.AddRange(childImports);

                importedPrograms.Add(parsedDocument);
            }
            return importedPrograms;
        }

    }
}
