lexer grammar AssertiveLexer;

VAR : '$' ID;
INT : MINUS? [0-9]+;
BOOL : 'true' | 'false';
IF : 'if';
ELSE : 'else';
WHILE : 'while';
LOOP: 'loop';
FROM: 'from';
TO: 'to';
PARALLEL: 'parallel';
EACH: 'each' ;
IN: 'in';
ASSERT : 'assert';
COLON: ':';
SEMICOLON : ';';
EQUALS : '=';
NOTEQUALS : '!=';
AND: 'and' ;
OR: 'or' ;
NOT: 'not';
LESSOREQUALTHAN: '<=';
MOREOREQUALTHAN: '>=';
LESSTHAN: '<';
MORETHAN: '>';
PLUS : '+';
MINUS : '-';
MULTIPLY : '*';
DIVIDE : '/';
LPAREN : '(';
RPAREN : ')';
DOUBLELBRACE : '{{' -> pushMode(DEFAULT_MODE);
DOUBLERBRACE : '}}' -> popMode;
LBRACE : '{' ;
RBRACE : '}' ;
LBRACK: '[' ;
RBRACK: ']' ;
COMMA : ',';
HOST_SECTION: 'host';
PATH_SECTION: 'path';
QUERY_SECTION : 'query';
HEADER_SECTION : 'headers';
BODY_SECTION : 'body';
DEF: 'def';
RETURN: 'return';
BREAK: 'break';
CONTINUE: 'continue';
IMPORT: 'import';

//predefined functions
OUT: 'out';

//content types
STRING: 'string';
FORMURLENCODED: 'formurlencoded';
FORMDATA: 'formdata';
STREAM: 'stream';

//http verbs
GET : 'GET';
POST : 'POST';
PUT : 'PUT';
DELETE : 'DELETE';
PATCH : 'PATCH';
OPTIONS : 'OPTIONS';
HEAD : 'HEAD';
TRACE : 'TRACE';
CONNECT : 'CONNECT';

ID: [a-zA-Z_][a-zA-Z0-9_]* ;
DQUOTE: '"' -> pushMode(IN_DQUOTE_STRING);
SQUOTE: '\'' -> pushMode(IN_SQUOTE_STRING);



//comments and whitespace
LINECOMMENT : '//' ~[\r\n]* -> channel(HIDDEN);
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
WS: [ \n\t\r]+ -> skip;

//double quoted strings
mode IN_DQUOTE_STRING;
DQUOTE_START_EXPRESSION: '{{' -> pushMode(DEFAULT_MODE);
DQUOTE_LBRACE: '{'; //allow single lbrace to be part of string content parser rule
DQUOTE_ESCAPE_SEQUENCE: '\\{' . ;
DQUOTE_IN_STRING: '"' -> type(DQUOTE), popMode;
DQUOTE_TEXT: ~[{\\"]+ ;

//single quoted strings
mode IN_SQUOTE_STRING;
SQUOTE_START_EXPRESSION: '{{' -> pushMode(DEFAULT_MODE);
SQUOTE_LBRACE: '{'; //allow single lbrace to be part of string content parser rule
SQUOTE_ESCAPE_SEQUENCE: '\\{' . ;
SQUOTE_IN_STRING: '\'' -> type(SQUOTE), popMode;
SQUOTE_TEXT: ~[{\\']+ ; 
