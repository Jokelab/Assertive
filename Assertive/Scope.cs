using Assertive.Models;
using Assertive.Types;
using static AssertiveParser;

namespace Assertive
{
    public class Scope
    {
        private Dictionary<string, Value> _variables = [];
        private Dictionary<string, FunctionModel> _functions = [];
        private readonly Scope? _parentScope;

        public bool ProtectedScope { get; set; } = false;

        public Scope(Scope? parentScope = null)
        {
            _parentScope = parentScope;
        }

        public Value? GetVariable(string name)
        {
            if (_variables.ContainsKey(name)) return _variables[name];
            if (_parentScope != null) return _parentScope.GetVariable(name);
            return null;
        }

        private Scope? GetVariableScope(string name)
        {
            if (_variables.ContainsKey(name)) return this;
            if (_parentScope != null) return _parentScope.GetVariableScope(name);
            return null;
        }

        public void StoreVariable(string name, Value value)
        {
            var existingScope = GetVariableScope(name);
            if (existingScope != this && ProtectedScope)
            {
                //variable was found in parent scope but we don't want to update parent variables when the scope is flagged as protected
                throw new InvalidOperationException("Not allowed to overwrite variable in parent of protected scope");
            }
            if (existingScope != null)
            {
                //store in current or parent scope
                existingScope._variables[name] = value;
            }
            else
            {
                //store in current scope
                StoreVariableInCurrentScope(name, value);
            }

        }

        public void StoreVariableInCurrentScope(string name, Value value)
        {
            //store in current scope
            _variables[name] = value;
        }

        public FunctionStatementContext? GetFunction(string name)
        {
            if (_functions.ContainsKey(name)) return _functions[name].Context;
            if (_parentScope != null) return _parentScope.GetFunction(name);
            return null;
        }

        public void StoreFunction(string name, FunctionStatementContext functionStatementContext)
        {
            _functions[name] = new FunctionModel(functionStatementContext);
        }

        public Scope? GetParent() { return _parentScope; }


        private bool _breakFlag = false;
        public void RaiseBreakFlag() { _breakFlag = true; }
        public void ResetBreakFlag() { _breakFlag = false; }
        public bool BreakFlagRaised => _breakFlag;

        private bool _continueFlag = false;
        public void RaiseContinueFlag() { _continueFlag = true; }
        public void ResetContinueFlag() { _continueFlag = false; }


        public bool ContinueFlagRaised => _continueFlag;

        /// <summary>
        /// Get all assert functions from the current scope down to the root scope
        /// </summary>
        /// <returns></returns>
        public List<FunctionModel> GetAssertFunctions()
        {
            var functions = new List<FunctionModel>();
            functions.AddRange(_functions.Where(f => f.Value.IsAssertFunction).Select(f => f.Value));

            var parent = GetParent();
            while (parent != null)
            {
                functions.AddRange(_functions.Where(f => f.Value.IsAssertFunction).Select(f => f.Value));
                parent = parent.GetParent();
            }
            return functions;

        }
    }
}