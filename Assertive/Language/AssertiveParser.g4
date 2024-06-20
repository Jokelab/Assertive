parser grammar AssertiveParser;

options {   tokenVocab = AssertiveLexer; }

program : imports = importStatements statement* EOF;

importStatements: importStatement*;

statement : 
           outputStatement
          | functionStatement
          | returnStatement
          | breakStatement
          | continueStatement
          | functionInvocationStatement
          | requestInvocationStatement
          | assignmentStatement
          | ifStatement
          | whileStatement
          | loopStatement
          | eachStatement
          | assertStatement
          ;


expression : VAR                                                                #VarExpression
           | INT                                                                #NumericExpression
           | BOOL                                                               #BoolExpression
           | string                                                             #StringExpression
           | LBRACE dictionary RBRACE                                           #DictionaryExpression
           | LBRACK (expression (COMMA expression)*)? RBRACK                    #ListExpression
           | requestInvocation                                                  #requestInvocationExpression        
           | functionInvocation                                                 #FunctionInvocationExpression
           | LPAREN expression RPAREN                                           #ParenthesesExpression
           | unaryOperator expression                                           #UnaryLogicalExpression
           | operandLeft = expression binaryArithmeticOperator operandRight = expression  #BinaryArithmeticExpression
           | operandLeft = expression binaryLogicalOperator operandRight = expression  #BinaryLogicalExpression
           ;


unaryOperator:  NOT;

binaryLogicalOperator: AND | OR ;

binaryArithmeticOperator: MULTIPLY
                | DIVIDE
                | PLUS
                | MINUS
                | MORETHAN
                | MOREOREQUALTHAN
                | LESSTHAN
                | LESSOREQUALTHAN
                | EQUALS
                | NOTEQUALS
                ;


assignmentStatement : VAR EQUALS expression SEMICOLON;

importStatement: IMPORT string SEMICOLON;

returnStatement: RETURN expression? SEMICOLON;

breakStatement: BREAK SEMICOLON;

continueStatement: CONTINUE SEMICOLON;

assertStatement : ASSERT expression description = string? SEMICOLON;

outputStatement: OUT expression SEMICOLON;

ifStatement : IF LPAREN expression RPAREN LBRACE exprTrue = ifBody RBRACE (ELSE LBRACE exprFalse = ifBody RBRACE)?;

ifBody : statement*;

loopStatement : LOOP VAR? FROM fromExp = expression TO toExp = expression (PARALLEL parExpression = expression)? LBRACE block = statement* RBRACE;

whileStatement : WHILE LPAREN expression RPAREN LBRACE block = statement* RBRACE;

eachStatement : EACH LPAREN VAR IN expression RPAREN LBRACE block = statement* RBRACE;

// http request start
httpMethod : GET | POST | PUT | DELETE | PATCH | OPTIONS | HEAD | TRACE | CONNECT | string;

requestInvocation : httpMethod uri = string (querySection | headerSection | bodySection)*;

requestInvocationStatement: requestInvocation SEMICOLON;

querySection : QUERY_SECTION expression;

headerSection : HEADER_SECTION expression;

dictionary: (dictionaryEntry (COMMA dictionaryEntry)*)?;

dictionaryEntry : key = expression COLON value = expression;

bodySection : BODY_SECTION contentType = (STRING | FORMURLENCODED | FORMDATA | STREAM)? expression;

// http request end

// function start
functionStatement: DEF functionName = ID string? (LPAREN functionParameterList? RPAREN)? (ASSERT assertFunction = ID)? LBRACE block = statement* RBRACE ;
functionParameterList: functionParam (COMMA functionParam)* ;
functionParam: VAR ;
functionInvocation: ID (LPAREN (expression (COMMA expression)*)? RPAREN)? ;
functionInvocationStatement: functionInvocation SEMICOLON;
//function end

string:   DQUOTE stringContent* DQUOTE
        | SQUOTE stringContent* SQUOTE;

stringContent :  DQUOTE_TEXT
               | DQUOTE_ESCAPE_SEQUENCE
               | DQUOTE_START_EXPRESSION expression DOUBLERBRACE
               | DQUOTE_LBRACE

               | SQUOTE_TEXT
               | SQUOTE_ESCAPE_SEQUENCE
               | SQUOTE_START_EXPRESSION expression DOUBLERBRACE
               | SQUOTE_LBRACE
               ;

