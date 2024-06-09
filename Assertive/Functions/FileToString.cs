using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    internal class FileToString : IFunction
    {
        private readonly IFileSystemService _fileSystemService;
        public FileToString(IFileSystemService fileSystemService)
        {
            _fileSystemService = fileSystemService;
        }
        public int ParameterCount => 1;

        public Task<Value> Execute(List<Value> values, FunctionContext context)
        {
            if (values[0] is not StringValue pathExpression)
            {
                throw new FunctionExecutionException("Path should be a string", this);
            }
            var path = pathExpression.ToString();
            if (!string.IsNullOrEmpty(context.FilePath))
            {
                path = _fileSystemService.CalculateRelativePath(context.FilePath, path);
            }

            var content = _fileSystemService.GetFileContent(path);
            return Task.FromResult<Value>(new StringValue(content));
        }
    }
}
