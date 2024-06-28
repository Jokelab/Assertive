using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Assertive.Functions;
using Assertive.Models;
using Assertive.Types;

namespace Assertive
{
    public class AnalyserVisitor : AssertiveParserBaseVisitor<Value>
    {
        public List<SemanticErrorModel> SemanticErrors { get; } = [];
        private readonly FunctionFactory _functionFactory;
        public string? FilePath { get; set; }
        private readonly Stack<Scope> _scopes = [];


        public AnalyserVisitor(FunctionFactory functionFactory)
        {
            _functionFactory = functionFactory;
            _scopes.Push(new Scope());
        }

        protected override Value DefaultResult => new VoidValue();

        public override Value VisitProgram([NotNull] AssertiveParser.ProgramContext context)
        {
            var rootScope = _scopes.Peek();

            //look ahead for function declarations
            RegisterFunctionStatementsInScope(rootScope, context.statement());

            var blockResult = ExecuteStatementBlock(rootScope, context.statement());

            return blockResult.ReturnValue;
        }

        public StatementBlockResult ExecuteStatementBlock(Scope scope, IEnumerable<AssertiveParser.StatementContext> statements)
        {
            var result = new StatementBlockResult()
            {
                ReturnValue = new VoidValue()
            };
            //visit each statement in the block
            foreach (var blockStatement in statements)
            {
                Visit(blockStatement);
            }
            return result;
        }

        private void RegisterFunctionStatementsInScope(Scope scope, IEnumerable<ParserRuleContext> context)
        {
            foreach (var rule in context)
            {
                var functionVisitor = new FunctionStatementVisitor();
                var functions = functionVisitor.Visit(rule);
                foreach (var function in functions)
                {
                    if (_functionFactory.BuiltInFunctionExists(function.Name))
                    {
                        AddError($"Function {function.Name} is already defined", rule, ErrorCodes.FunctionAlreadyDefined);
                    }
                    scope.StoreFunction(function.Name, function.Context);
                }
            }
        }

        public override Value VisitFunctionStatement([NotNull] AssertiveParser.FunctionStatementContext context)
        {

            var functionScope = new Scope(_scopes.Peek());
            _scopes.Push(functionScope);

            //add function parameter values to the scope
            var expectedParameterCount = context.functionParameterList() == null
                      ? 0
                      : context.functionParameterList().functionParam().Length;
            for (var i = 0; i < expectedParameterCount; i++)
            {
                var functionParam = context.functionParameterList().functionParam()[i];
                var paramName = functionParam.VAR().GetText();
                var paramValue = new VoidValue();
                functionScope.StoreVariableInCurrentScope(paramName, paramValue);
            }


            RegisterFunctionStatementsInScope(functionScope, context.statement());
            ExecuteStatementBlock(functionScope, context.statement());
            _scopes.Pop();
            return new VoidValue();
        }

        public override Value VisitFunctionInvocation([NotNull] AssertiveParser.FunctionInvocationContext context)
        {
            Value returnValue = new VoidValue();
            var functionName = context.ID().GetText();

            //check if it is a built in function
            var function = _functionFactory.GetFunction(functionName);
            if (function != null)
            {
                var actualParamCount = context.expression().Length;
                if (function.ParameterCount != actualParamCount)
                {
                    AddError($"Expected {function.ParameterCount} but received {actualParamCount} parameters for built-in function {functionName}", context, ErrorCodes.FunctionParamsMismatch);
                }
            }
            else
            {
                //user defined function
                var functionStatement = _scopes.Peek().GetFunction(functionName);
                if (functionStatement != null)
                {
                    var expectedParameterCount = functionStatement.functionParameterList() == null
                        ? 0
                        : functionStatement.functionParameterList().functionParam().Length;

                    var actualParamCount = context.expression().Length;
                    if (expectedParameterCount != actualParamCount)
                    {
                        AddError($"Expected {expectedParameterCount} but received {actualParamCount} parameters for function {functionName}", context, ErrorCodes.FunctionParamsMismatch);
                    }
                }
                else
                {
                    AddError($"Function '{functionName}' called but it was not found in the current context", context, ErrorCodes.FunctionNotFound);
                }
            }
            return returnValue;
        }

        public override Value VisitAssignmentStatement([NotNull] AssertiveParser.AssignmentStatementContext context)
        {
            var val = Visit(context.expression());
            var scope = _scopes.Peek();
            scope.StoreVariable(context.VAR().GetText(), val);
            return val;
        }

        public override Value VisitVarExpression([NotNull] AssertiveParser.VarExpressionContext context)
        {
            var scope = _scopes.Peek();
            var variable = scope.GetVariable(context.GetText());
            if (variable == null)
            {
                AddError($"Variable '{context.GetText()}' not found in the current context", context, ErrorCodes.VariableNotFound);
            }
            return new VoidValue();
        }

        public override Value VisitEachStatement([NotNull] AssertiveParser.EachStatementContext context)
        {
            Visit(context.expression());

            var loopVarName = context.VAR().GetText();
            var loopScope = new Scope(_scopes.Peek());
            _scopes.Push(loopScope);

            //set the loop variable
            loopScope.StoreVariableInCurrentScope(loopVarName, new VoidValue());

            //visit each statement in the loop body
            var blockResult = ExecuteStatementBlock(loopScope, context.statement());
            _scopes.Pop();

            return blockResult.ReturnValue;
        }

        public override Value VisitLoopStatement([NotNull] AssertiveParser.LoopStatementContext context)
        {
            Visit(context.fromExp);
            Visit(context.toExp);
            var loopScope = new Scope(_scopes.Peek());
            _scopes.Push(loopScope);

            //synchronous loop

            if (context.VAR() != null)
            {
                //set the loop variable
                var loopVarName = context.VAR().GetText();
                loopScope.StoreVariableInCurrentScope(loopVarName, new VoidValue());
            }

            if (context.parExpression != null)
            {
                Visit(context.parExpression);
            }
            //visit each statement in the loop body
            var blockResult = ExecuteStatementBlock(loopScope, context.statement());
            var output = blockResult.ReturnValue;

            _scopes.Pop();

            return output;
        }

        private void AddError(string message, ParserRuleContext context, string errorCode)
        {
            SemanticErrors.Add(new SemanticErrorModel() { Context = context, Message = message, FilePath = FilePath, ErrorCode = errorCode });
        }
    }
}
