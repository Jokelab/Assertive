using Assertive.Models;
using Assertive.Requests.Http;
using Assertive.Types;
using static AssertiveParser;

namespace Assertive
{
    public class Scope
    {
        private Dictionary<string, Value> _variables = [];
        private Dictionary<string, FunctionModel> _functions = [];
        private readonly Scope? _parentScope;
        public bool IsAnnotatedFunctionScope = false;
        private List<HttpRequest> _recordedRequests = [];
        private List<AssertResult> _recordedAsserts = [];

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

        public void RecordRequest(HttpRequest request)
        {
            //store request in every scope of an annotated function
            var scope = this;
            while (scope != null)
            {
                if (scope.IsAnnotatedFunctionScope)
                {
                    scope._recordedRequests.Add(request);
                }
                scope = scope.GetParent();
            }
        }

        public long RecordedRequestDuration => _recordedRequests.Sum(x => x.DurationMs);

        public long TotalRequests => _recordedRequests.Count;

        public void RecordAssert(AssertResult assertResult)
        {
            //store assert in every scope of an annotated function
            var scope = this;
            while (scope != null)
            {
                if (scope.IsAnnotatedFunctionScope)
                {
                    scope._recordedAsserts.Add(assertResult);
                }
                scope = scope.GetParent();
            }
        }
    }
}