using Antlr4.Runtime;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using Assertive.Exceptions;
using Assertive.Functions;
using Assertive.Models;
using Assertive.Requests;
using Assertive.Requests.Http;
using Assertive.Types;
using Microsoft.Extensions.Logging;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;
using System.Web;

namespace Assertive
{
    public class ProgramVisitor : AssertiveParserBaseVisitor<Task<Value>>
    {
        private IRequestDispatcher _requestDispatcher;
        private readonly FunctionFactory _functionFactory;
        private readonly IEnumerable<IOutputWriter> _outputWriters;
        private readonly ILogger<ProgramVisitor> _logger;
        private readonly Stack<Scope> _scopes = new();
        public ProgramVisitor(IRequestDispatcher requestDispatcher, FunctionFactory functionFactory, IEnumerable<IOutputWriter> outputWriters, ILogger<ProgramVisitor> logger)
        {
            _requestDispatcher = requestDispatcher;
            _functionFactory = functionFactory;
            _outputWriters = outputWriters;
            _logger = logger;
            _scopes.Push(new Scope());
        }

        /// <summary>
        /// A filepath that points to the Assertive document that is currently being parsed. If empty, it means that the parser is activated without a file path.
        /// </summary>
        public string? FilePath { get; set; }

        protected override Task<Value> DefaultResult => Task.FromResult<Value>(new VoidValue());

        /// <summary>
        /// Entry point rule of an Assertive program
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<Value> VisitProgram([NotNull] AssertiveParser.ProgramContext context)
        {
            var rootScope = _scopes.Peek();

            //look ahead for function declarations
            RegisterFunctionStatementsInScope(rootScope, context.statement());

            var blockResult = await ExecuteStatementBlock(rootScope, context.statement()).ConfigureAwait(false);

            return blockResult.ReturnValue;
        }


        #region Statements

        public override Task<Value> VisitFunctionStatement([NotNull] AssertiveParser.FunctionStatementContext context)
        {
            var scope = _scopes.Peek();
            scope.StoreFunction(context.ID().GetText(), context);
            return Task.FromResult<Value>(new VoidValue());
        }

        public override async Task<Value> VisitOutputStatement([NotNull] AssertiveParser.OutputStatementContext context)
        {
            var output = await Visit(context.expression()).ConfigureAwait(false);
            foreach (var writer in _outputWriters)
            {
                await writer.Write(output.ToString()!).ConfigureAwait(false);
            }
            return new VoidValue();
        }

        public override async Task<Value> VisitAssignmentStatement([NotNull] AssertiveParser.AssignmentStatementContext context)
        {
            var val = await Visit(context.expression()).ConfigureAwait(false);
            var scope = _scopes.Peek();
            scope.StoreVariable(context.VAR().GetText(), val);
            return val;
        }


        public override async Task<Value> VisitAssertStatement([NotNull] AssertiveParser.AssertStatementContext context)
        {
            var expressionResult = await Visit(context.expression()).ConfigureAwait(false);
            var boolExpressionResult = expressionResult as BooleanValue;

            if (boolExpressionResult != null)
            {
                var assertResult = new AssertResult
                {
                    Passed = boolExpressionResult.Value,
                    ExpressionText = context.expression().GetText(),
                    Description = ""
                };

                //overwrite description if available 
                if (context.description != null)
                {
                    var description = await Visit(context.description) as StringValue;
                    if (description != null)
                    {
                        assertResult.Description = description.Value;
                    }
                    else
                    {
                        throw new InterpretationException("Assert descriptions should be string values", context.description, FilePath);
                    }
                }

                foreach (var outputWriter in _outputWriters)
                {
                    await outputWriter.Assertion(assertResult);
                }
            }
            else
            {
                throw new InterpretationException("Assert expressions should be boolean values", context.expression(), FilePath);
            }
            return expressionResult;
        }

        public override async Task<Value> VisitReturnStatement([NotNull] AssertiveParser.ReturnStatementContext context)
        {
            var result = await Visit(context.expression()).ConfigureAwait(false);
            _scopes.Peek().RaiseBreakFlag(); //return is a breaking statement
            return result;
        }

        public override Task<Value> VisitBreakStatement([NotNull] AssertiveParser.BreakStatementContext context)
        {
            _scopes.Peek().RaiseBreakFlag();
            return Task.FromResult<Value>(new VoidValue());
        }

        public override Task<Value> VisitContinueStatement([NotNull] AssertiveParser.ContinueStatementContext context)
        {
            _scopes.Peek().RaiseContinueFlag();
            return Task.FromResult<Value>(new VoidValue());
        }

        public override async Task<Value> VisitIfStatement([NotNull] AssertiveParser.IfStatementContext context)
        {
            var result = await Visit(context.expression()).ConfigureAwait(false);
            Value output = new VoidValue();
            if (result is not BooleanValue boolExpression) throw new InterpretationException("If conditon expression should be a boolean expression", context.expression(), FilePath);
            if (boolExpression.Value)
            {
                output = await Visit(context.exprTrue).ConfigureAwait(false);
            }
            else if (context.exprFalse != null)
            {

                output = await Visit(context.exprFalse).ConfigureAwait(false);
            }
            return output;
        }

        public override async Task<Value> VisitWhileStatement([NotNull] AssertiveParser.WhileStatementContext context)
        {

            var loopConditionResult = await Visit(context.expression()).ConfigureAwait(false);
            if (loopConditionResult is not BooleanValue loopConditionExpression) throw new InterpretationException("While expression should be a boolean expression", context.expression(), FilePath);

            Value output = new VoidValue();
            var loopScope = new Scope(_scopes.Peek());
            _scopes.Push(loopScope);
            while (loopConditionExpression != null && loopConditionExpression.Value)
            {
                //visit each statement in the loop body
                var blockResult = await ExecuteStatementBlock(loopScope, context.statement()).ConfigureAwait(false);
                output = blockResult.ReturnValue;
                if (blockResult.ShouldBreak) break;
                if (blockResult.ShouldContinue) continue;

                //re-evaluate the expression
                loopConditionExpression = (await Visit(context.expression()).ConfigureAwait(false) as BooleanValue)!;
            }
            _scopes.Pop();
            return output;

        }

        public override async Task<Value> VisitLoopStatement([NotNull] AssertiveParser.LoopStatementContext context)
        {
            var from = await Visit(context.fromExp).ConfigureAwait(false) as NumericValue;
            if (from == null) throw new InterpretationException("'from' expression should be numeric", context.fromExp, FilePath);
            var to = await Visit(context.toExp).ConfigureAwait(false) as NumericValue;
            if (to == null) throw new InterpretationException("'to' expression should be numeric", context.fromExp, FilePath);

            if (context.parExpression == null)
            {
                var loopScope = new Scope(_scopes.Peek());
                _scopes.Push(loopScope);
                Value output = new VoidValue();
                //synchronous loop
                for (var i = from.Value; i <= to.Value; i++)
                {
                    if (context.VAR() != null)
                    {
                        var loopVarName = context.VAR().GetText();
                        loopScope.StoreVariableInCurrentScope(loopVarName, new NumericValue(i));
                    }
                    //visit each statement in the loop body
                    var blockResult = await ExecuteStatementBlock(loopScope, context.statement()).ConfigureAwait(false);
                    output = blockResult.ReturnValue;
                    if (blockResult.ShouldBreak) break;
                    if (blockResult.ShouldContinue) continue;

                }
                _scopes.Pop();
                return output;
            }
            else
            {
                //parallel loop
                var maxParallel = await Visit(context.parExpression).ConfigureAwait(false) as NumericValue;
                if (maxParallel == null) throw new InterpretationException("'parallel' expression should be numeric", context.fromExp, FilePath);
                ParallelOptions parallelOptions = new()
                {
                    MaxDegreeOfParallelism = maxParallel.Value
                };

                await Parallel.ForAsync(from.Value, to.Value + 1, parallelOptions, async (i, ct) =>
                {
                    var protectedScope = new Scope(_scopes.Peek()) { ProtectedScope = true };
                    if (context.VAR() != null)
                    {
                        var loopVarName = context.VAR().GetText();
                        protectedScope.StoreVariableInCurrentScope(loopVarName, new NumericValue(i));
                    }
                    var localVisitor = new ProgramVisitor(_requestDispatcher, _functionFactory, _outputWriters, _logger)
                    {
                        FilePath = FilePath
                    };
                    localVisitor._scopes.Push(protectedScope);
                    await localVisitor.ExecuteStatementBlock(protectedScope, context.statement()).ConfigureAwait(false);
                });

                return new VoidValue();
            }
        }

        public override async Task<Value> VisitEachStatement([NotNull] AssertiveParser.EachStatementContext context)
        {
            var list = await Visit(context.expression()).ConfigureAwait(false) as ListValue;
            if (list == null) throw new InterpretationException("Expression in each-statement should be a list", context.expression(), FilePath);

            Value output = new VoidValue();
            var loopVarName = context.VAR().GetText();
            var loopScope = new Scope(_scopes.Peek());
            _scopes.Push(loopScope);
            for (var i = 0; i < list.ListValues.Count; i++)
            {
                //set the loop variable for each iteration
                loopScope.StoreVariableInCurrentScope(loopVarName, list.ListValues[i]);

                //visit each statement in the loop body
                var blockResult = await ExecuteStatementBlock(loopScope, context.statement()).ConfigureAwait(false);
                output = blockResult.ReturnValue;
                if (blockResult.ShouldBreak) break;
                if (blockResult.ShouldContinue) continue;
            }
            _scopes.Pop();

            return output;
        }

        #endregion

        #region Expressions
        public override async Task<Value> VisitUnaryLogicalExpression([NotNull] AssertiveParser.UnaryLogicalExpressionContext context)
        {
            if (context.unaryOperator().GetChild(0).Payload is not IToken opToken)
                throw new InterpretationException("Unary operator not present in expression", context, FilePath);

            var expressionValue = await Visit(context.expression()).ConfigureAwait(false) as BooleanValue;
            if (expressionValue == null) throw new InterpretationException("Expected a boolean expression for logical operators", context.expression(), FilePath);
            if (opToken.Type == AssertiveLexer.NOT)
            {
                return new BooleanValue(!expressionValue.Value);
            }
            throw new InterpretationException($"Unknown unary logical operator {opToken.Text}", context, FilePath);
        }


        public override async Task<Value> VisitBinaryLogicalExpression([NotNull] AssertiveParser.BinaryLogicalExpressionContext context)
        {
            if (context.binaryLogicalOperator().GetChild(0).Payload is not IToken opToken)
                throw new InterpretationException("Binary operator not present in expression", context, FilePath);

            var leftValue = await Visit(context.operandLeft).ConfigureAwait(false) as BooleanValue;
            if (leftValue == null) throw new InterpretationException("Expected a boolean value for left side of logical expression", context, FilePath);

            switch (opToken.Type)
            {

                case AssertiveLexer.AND:
                    {
                        var rightValue = await Visit(context.operandRight).ConfigureAwait(false) as BooleanValue;
                        if (rightValue == null) throw new InterpretationException("Expected a boolean value for right side of 'and' expression", context.operandRight, FilePath);
                        return new BooleanValue(leftValue.Value && rightValue.Value);

                    }
                case AssertiveLexer.OR:
                    {
                        if (leftValue.Value) return new BooleanValue(true); //immediately return if first operand is true

                        var rightValue = await Visit(context.operandRight).ConfigureAwait(false) as BooleanValue;
                        if (rightValue == null) throw new InterpretationException("Expected a boolean value for right side of 'or' expression", context.operandRight, FilePath);
                        return new BooleanValue(leftValue.Value || rightValue.Value);
                    }
                default: throw new InterpretationException($"Unsupported logical operator {opToken.Type} Text: {opToken.Text}", context, FilePath);
            }
        }

        public override Task<Value> VisitParenthesesExpression([NotNull] AssertiveParser.ParenthesesExpressionContext context)
        {
            return Visit(context.expression());
        }

        public override Task<Value> VisitBoolExpression([NotNull] AssertiveParser.BoolExpressionContext context)
        {
            return Task.FromResult<Value>(new BooleanValue(context.GetText()));
        }

        public override Task<Value> VisitNumericExpression([NotNull] AssertiveParser.NumericExpressionContext context)
        {
            return Task.FromResult<Value>(new NumericValue(context.GetText()));
        }

        public override Task<Value> VisitStringExpression([NotNull] AssertiveParser.StringExpressionContext context)
        {
            return Visit(context.@string());
        }

        protected override bool ShouldVisitNextChild(IRuleNode node, Task<Value> currentResult)
        {
            if (_scopes.Peek().BreakFlagRaised)
            {
                return false;
            }
            return base.ShouldVisitNextChild(node, currentResult);
        }

        public override async Task<Value> VisitListExpression([NotNull] AssertiveParser.ListExpressionContext context)
        {
            var list = new List<Value>();
            foreach (var item in context.expression())
            {
                list.Add(await Visit(item).ConfigureAwait(false));
            }
            return new ListValue(list);
        }

        public override async Task<Value> VisitDictionaryExpression([NotNull] AssertiveParser.DictionaryExpressionContext context)
        {
            return await Visit(context.dictionary()).ConfigureAwait(false);
        }

        public override async Task<Value> VisitDictionary([NotNull] AssertiveParser.DictionaryContext context)
        {
            var dic = new DictionaryValue();
            foreach (var dicEntry in context.dictionaryEntry())
            {
                var entryResult = await Visit(dicEntry).ConfigureAwait(false);
                if (entryResult is DictionaryEntry dicEntryResult)
                {
                    dic.AddEntry(dicEntryResult);
                }
                else
                {
                    throw new InterpretationException("Only dictionary entries allowed inside a dictionary expression", dicEntry, FilePath);
                }
            }
            return dic;
        }

        public override async Task<Value> VisitDictionaryEntry([NotNull] AssertiveParser.DictionaryEntryContext context)
        {
            var keyResult = await Visit(context.key);
            if (string.IsNullOrEmpty(keyResult.ToString()))
            {
                throw new InterpretationException("Dictionary key may not be an empty string", context.key, FilePath);
            }
            var valueResult = await Visit(context.value);
            return new DictionaryEntry() { Key = keyResult, Value = valueResult };
        }

        public override async Task<Value> VisitString([NotNull] AssertiveParser.StringContext context)
        {
            StringBuilder output = new();
            foreach (var stringPart in context.stringContent())
            {
                var strValue = await Visit(stringPart).ConfigureAwait(false) as StringValue;
                if (strValue != null)
                {
                    output.Append(strValue.Value);
                }
            }
            return new StringValue(output.ToString());

        }

        public override async Task<Value> VisitStringContent([NotNull] AssertiveParser.StringContentContext context)
        {
            if (context.expression() != null)
            {
                var expressionInString = (await Visit(context.expression()).ConfigureAwait(false)).ToString();
                return new StringValue(expressionInString!);
            }
            return new StringValue(context.GetText());
        }

        public override Task<Value> VisitVarExpression([NotNull] AssertiveParser.VarExpressionContext context)
        {
            var scope = _scopes.Peek();
            var variable = scope.GetVariable(context.GetText());
            if (variable == null) throw new InterpretationException($"Variable {context.GetText()} not found", context, FilePath);
            return Task.FromResult(variable);
        }


        public override async Task<Value> VisitBinaryArithmeticExpression([NotNull] AssertiveParser.BinaryArithmeticExpressionContext context)
        {
            if (context.binaryArithmeticOperator().GetChild(0).Payload is not IToken opToken)
                throw new InterpretationException("Binary operator not present in expression", context, FilePath);

            var leftValue = await Visit(context.operandLeft).ConfigureAwait(false) as NumericValue;
            if (leftValue == null) throw new InterpretationException("Expected a numeric value for left side of arithmetic expression", context.operandLeft, FilePath);

            var rightValue = await Visit(context.operandRight).ConfigureAwait(false) as NumericValue;
            if (rightValue == null) throw new InterpretationException("Expected a numeric value for right side of arithmetic expression", context.operandRight, FilePath);

            switch (opToken.Type)
            {
                case AssertiveLexer.MULTIPLY: return new NumericValue(leftValue.Value * rightValue.Value);
                case AssertiveLexer.DIVIDE: return new NumericValue(leftValue.Value / rightValue.Value);
                case AssertiveLexer.PLUS: return new NumericValue(leftValue.Value + rightValue.Value);
                case AssertiveLexer.MINUS: return new NumericValue(leftValue.Value - rightValue.Value);
                case AssertiveLexer.MORETHAN: return new BooleanValue(leftValue.Value > rightValue.Value);
                case AssertiveLexer.MOREOREQUALTHAN: return new BooleanValue(leftValue.Value >= rightValue.Value);
                case AssertiveLexer.LESSTHAN: return new BooleanValue(leftValue.Value < rightValue.Value);
                case AssertiveLexer.LESSOREQUALTHAN: return new BooleanValue(leftValue.Value <= rightValue.Value);
                case AssertiveLexer.EQUALS: return new BooleanValue(leftValue.Value == rightValue.Value);
                case AssertiveLexer.NOTEQUALS: return new BooleanValue(leftValue.Value != rightValue.Value);
                default: throw new InterpretationException($"Unsupported binary operator {opToken.Type} Text: {opToken.Text}", context, FilePath);
            }
        }

        public override async Task<Value> VisitFunctionInvocation([NotNull] AssertiveParser.FunctionInvocationContext context)
        {
            Value returnValue;
            var functionName = context.ID().GetText();

            //check if it is a built in function
            var function = _functionFactory.GetFunction(functionName);
            if (function != null)
            {
                var actualParamCount = context.expression().Length;
                if (function.ParameterCount != actualParamCount)
                {
                    throw new InterpretationException($"Expected {function.ParameterCount} but received {actualParamCount} parameters for built-in function {functionName}", context, FilePath);
                }
                var parameters = new List<Value>();
                for (var i = 0; i < function.ParameterCount; i++)
                {
                    var paramValue = await Visit(context.expression()[i]).ConfigureAwait(false); //calculate value to pass to the parameter
                    parameters.Add(paramValue);
                }
                try
                {
                    //run the built-in function
                    var functionContext = new FunctionContext() { FilePath = FilePath };
                    returnValue = await function.Execute(parameters, functionContext).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    throw new InterpretationException("Built-in function failed: " + exception.Message, context, FilePath, exception);
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
                        throw new InterpretationException($"Expected {expectedParameterCount} but received {actualParamCount} parameters for function {functionName}", context, FilePath);
                    }

                    //create new scope for the function block and its parameter variables
                    var functionScope = new Scope(_scopes.Peek());

                    //look ahead for function declarations inside this function
                    RegisterFunctionStatementsInScope(functionScope, functionStatement.statement());

                    _scopes.Push(functionScope);

                    //add function parameter values to the scope
                    for (var i = 0; i < expectedParameterCount; i++)
                    {
                        var functionParam = functionStatement.functionParameterList().functionParam()[i];
                        var paramName = functionParam.VAR().GetText();
                        var paramValue = await Visit(context.expression()[i]).ConfigureAwait(false); //calculate value to pass to the parameter
                        functionScope.StoreVariableInCurrentScope(paramName, paramValue);
                    }

                    StringValue? description = null;
                    if (functionStatement.@string() != null)
                    {
                        description = await Visit(functionStatement.@string()) as StringValue;
                        var annotatedFunctionStart = new AnnotatedFunction() { Invocation = "Start", Annotation = description!.Value, FunctionName = functionName };
                        foreach (var writer in _outputWriters) await writer.AnnotatedFunctionStart(annotatedFunctionStart);
                    }
                    var sw = new Stopwatch();
                    sw.Start();
                    var blockResult = await ExecuteStatementBlock(functionScope, functionStatement.statement()).ConfigureAwait(false);
                    sw.Stop();
                    if (description != null)
                    {
                        var annotatedFunctionEnd = new AnnotatedFunction() { Invocation = "End", Annotation = description.Value, FunctionName = functionName, DurationMs = (long)sw.Elapsed.TotalMilliseconds };
                        foreach (var writer in _outputWriters) await writer.AnnotatedFunctionEnd(annotatedFunctionEnd);
                    }
                    returnValue = blockResult.ReturnValue;

                    _scopes.Pop();
                }
                else
                {
                    throw new InterpretationException($"Function '{functionName}' called but it was not found in the current scope", context, FilePath);
                }
            }
            return returnValue;
        }



        #endregion

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
                        throw new InterpretationException($"Cannot register function {function.Name} because it already exists", rule, FilePath);
                    }
                    scope.StoreFunction(function.Name, function.Context);
                }
            }
        }

        public async Task<StatementBlockResult> ExecuteStatementBlock(Scope scope, IEnumerable<AssertiveParser.StatementContext> statements)
        {
            var result = new StatementBlockResult()
            {
                ReturnValue = new VoidValue()
            };
            //visit each statement in the block
            foreach (var blockStatement in statements)
            {
                var statementResult = await Visit(blockStatement).ConfigureAwait(false);

                if (statementResult != null)
                {
                    result.ReturnValue = statementResult;
                }
                if (scope.BreakFlagRaised)
                {
                    result.ShouldBreak = true;
                    scope.ResetBreakFlag();
                    break;
                }
                if (scope.ContinueFlagRaised)
                {
                    result.ShouldContinue = true;
                    scope.ResetContinueFlag();
                    break;
                }


            }
            return result;
        }

        public override async Task<Value> VisitChildren(IRuleNode node)
        {
            Task<Value> val = DefaultResult;
            int childCount = node.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                if (!ShouldVisitNextChild(node, val))
                {
                    break;
                }

                var nextResult = await node.GetChild(i).Accept(this);
                val = AggregateResult(val, Task.FromResult(nextResult));
            }

            return await val.ConfigureAwait(false);
        }

        public override async Task<Value> VisitRequestInvocation([NotNull] AssertiveParser.RequestInvocationContext context)
        {
            var requestMessage = new HttpRequestMessage();
            if (context.httpMethod().@string() == null)
            {
                //static method
                requestMessage.Method = HttpMethod.Parse(context.httpMethod().GetText());
            }
            else
            {
                //dynamic method
                var dynamicMethod = await Visit(context.httpMethod().@string()).ConfigureAwait(false);
                if (dynamicMethod is StringValue strMethod)
                {
                    requestMessage.Method = HttpMethod.Parse(strMethod.Value);
                }
                else
                {
                    throw new InterpretationException("Expected a string value when using a dynamic http request method", context.httpMethod(), FilePath);
                }

            }

            try
            {
                requestMessage.RequestUri = new Uri((await Visit(context.uri).ConfigureAwait(false)).ToString()!);
            }
            catch (UriFormatException uriException)
            {
                throw new InterpretationException(uriException.Message, context.uri, FilePath, uriException);
            }

            if (context.querySection().Length == 1)
            {
                var queryExpressionResult = await Visit(context.querySection()[0].expression());
                if (queryExpressionResult is DictionaryValue dicValue)
                {
                    var nameValueCollection = new NameValueCollection();

                    foreach (var entry in dicValue.GetEntries())
                    {
                        nameValueCollection[entry.Key.ToString()] = entry.Value.ToString();
                    }

                    var queryString = string.Join("&", nameValueCollection.AllKeys.Select(a => a + "=" + HttpUtility.UrlEncode(nameValueCollection[a])));

                    UriBuilder uriBuilder = new UriBuilder(requestMessage.RequestUri)
                    {
                        Query = queryString
                    };

                    requestMessage.RequestUri = uriBuilder.Uri;

                }
                else
                {
                    throw new InterpretationException("Query expression must be a dictionary", context.headerSection()[0].expression(), FilePath);
                }
            }
            else if (context.querySection().Length > 1)
            {
                throw new InterpretationException("Only 1 query section allowed", context, FilePath);
            }

            if (context.headerSection().Length == 1)
            {
                var headerExpressionResult = await Visit(context.headerSection()[0].expression());
                if (headerExpressionResult is DictionaryValue dicValue)
                {
                    foreach (var entry in dicValue.GetEntries())
                    {
                        requestMessage.Headers.Add(entry.Key.ToString()!, entry.Value.ToString());
                    }
                }
                else
                {
                    throw new InterpretationException("Header expression must be a dictionary", context.headerSection()[0].expression(), FilePath);
                }
            }
            else if (context.headerSection().Length > 1)
            {
                throw new InterpretationException("Only 1 header section allowed", context, FilePath);
            }

            if (context.bodySection().Length == 1)
            {

                var bodyExpressionResult = await Visit(context.bodySection()[0].expression()).ConfigureAwait(false);
                if (bodyExpressionResult == null)
                {
                    throw new InterpretationException("No valid request body found", context, FilePath);
                }
                requestMessage.Content = RequestContentFactory.Create(context.bodySection()[0], bodyExpressionResult, FilePath);

            }
            else if (context.bodySection().Length > 1)
            {
                throw new InterpretationException("Only 1 body section allowed", context, FilePath);
            }

            var requestModel = _requestDispatcher.CreateRequest(requestMessage);

            foreach (var writer in _outputWriters)
            {
                await writer.RequestStart(requestModel).ConfigureAwait(false);
            }

            await _requestDispatcher.SendRequest(requestModel).ConfigureAwait(false);

            foreach (var writer in _outputWriters)
            {
                await writer.RequestEnd(requestModel).ConfigureAwait(false);
            }

            return new HttpRequestValue(requestModel);
        }
    }
}
