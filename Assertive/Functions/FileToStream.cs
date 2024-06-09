using Assertive.Exceptions;
using Assertive.Types;

namespace Assertive.Functions
{
    internal class FileToStream : IFunction
    {
        private readonly IFileSystemService _fileSystemService;
        public FileToStream(IFileSystemService fileSystemService)
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

            var stream = _fileSystemService.GetFileStream(path);
            return Task.FromResult<Value>(new StreamValue(stream, path));
        }
    }
}
