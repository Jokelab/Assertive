namespace Assertive.Functions
{
    /// <summary>
    /// The function factory contains an instance of each registered built in function.
    /// We don't currently use keyed services, because Omnisharp doesn't support this yet since it is on .NET6
    /// </summary>
    public class FunctionFactory
    {

        private readonly Dictionary<string, IFunction> _functionRegister = [];
        public FunctionFactory(IEnumerable<IFunction> functions)
        {
            foreach (IFunction function in functions)
            {
                var name = function.GetType().Name;
                if (BuiltInFunctionExists(name))
                    throw new InvalidOperationException($"Built in function {name} is already registered");
                _functionRegister[name] = function;
            }
        }

        public IFunction? GetFunction(string name)
        {
            if (!BuiltInFunctionExists(name)) return null;
            return _functionRegister[name];
        }

        public bool BuiltInFunctionExists(string name)
        {
            return _functionRegister.ContainsKey(name);
        }
    }

}
