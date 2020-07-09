grammar GLang;

/*
 * Parser Rules
 */
 
compilationUnit : globalStatement+;

declerationOrAssign: variableDeclarationStatement | assignmentExpression;
typeDefinition: TYPE_DELIMETER TYPE_NAME;

	/*
	 * Statements
	 */

	
globalStatement: functionDefinition | baseStatement;
baseStatement : forStatement | whileStatement | ifStatement | block | variableDeclarationStatement | expressionStatement;
statement : baseStatement | BREAK | CONTINUE;

definedIdentifier: IDENTIFIER typeDefinition;
seperatedDefinedIdentifier: definedIdentifier COMMA;

parametersDefinition: L_PARENTHESIS seperatedDefinedIdentifier+ last=definedIdentifier R_PARENTHESIS
					| L_PARENTHESIS last=definedIdentifier R_PARENTHESIS
					| L_PARENTHESIS R_PARENTHESIS;

functionDefinition: FUNCTION IDENTIFIER parametersDefinition typeDefinition statement;

block : L_BRACE statement* R_BRACE;

expressionStatement: assignmentExpression | binaryExpression;

forStatement: FOR L_PARENTHESIS declerationOrAssign COMMA binaryExpression COMMA assignmentExpression R_PARENTHESIS statement;

ifStatement : IF L_PARENTHESIS binaryExpression R_PARENTHESIS trueBranch=statement (ELSE falseBranch=statement)?;
whileStatement: WHILE L_PARENTHESIS binaryExpression R_PARENTHESIS body=statement;

variableDeclarationStatement: DECL_VARIABLE definedIdentifier
							| DECL_VARIABLE assignmentExpression;

assignmentExpression : IDENTIFIER typeDefinition? ASSIGNMENT_OPERATOR binaryExpression;


	/*
	 * Expressions
	 */

literal : DOUBLE | INTEGER | BOOLEAN | functionCall | IDENTIFIER | STRING;

unaryExpression : L_PARENTHESIS binaryExpression R_PARENTHESIS
				| op=(ADDITIVE_OP | BANG) unaryExpression
				| atom=literal;

binaryExpression: left=binaryExpression op=CARAT right=binaryExpression
				| left=binaryExpression op=MULTIPLICATIVE_OP right=binaryExpression
				| left=binaryExpression op=ADDITIVE_OP right=binaryExpression
				| left=binaryExpression op=LOW_PRIORITY_OP right=binaryExpression
				| left=binaryExpression op=COMPARE_OP right=binaryExpression
				| left=binaryExpression op=LOGICAL_OP right=binaryExpression
				| atom=unaryExpression;
				

seperatedExpression: binaryExpression COMMA;
functionCall: funcName=IDENTIFIER L_PARENTHESIS seperatedExpression+ last=binaryExpression R_PARENTHESIS
			| funcName=IDENTIFIER L_PARENTHESIS last=binaryExpression R_PARENTHESIS
			| funcName=IDENTIFIER L_PARENTHESIS R_PARENTHESIS;
/*
 * Lexer Rules
 */

fragment DIGIT : '0'..'9';
fragment TRUE : ('T' | 't') 'rue';
fragment FALSE : ('F' | 'f') 'alse';

fragment VAR: 'var';
fragment VAL: 'val';

fragment INTEGER_TYPE: 'int';
fragment DOUBLE_TYPE: 'double';
fragment BOOLEAN_TYPE: 'bool';
fragment STRING_TYPE: 'string';
fragment VOID_TYPE: 'void';

fragment EQUALITY_OPERATOR: '==';
fragment GREATER: '>=';
fragment LESSER: '<=';
fragment GREATER_STRICT: '>';
fragment LESSER_STRICT: '<';

fragment MOD_OPERATOR : 'mod';

STRING : '"' .*? '"';

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);

COMMA : ',';

TYPE_DELIMETER : ':';
BREAK: 'b' 'r' 'e' 'a' 'k';
CONTINUE : 'c' 'o' 'n' 't' 'i' 'n' 'u' 'e';
TYPE_NAME : INTEGER_TYPE | DOUBLE_TYPE | BOOLEAN_TYPE | STRING_TYPE | VOID_TYPE;

BOOLEAN: TRUE | FALSE;
DOUBLE: '.' DIGIT+ | DIGIT+ '.' DIGIT+;
INTEGER: DIGIT+;

DECL_VARIABLE : VAL | VAR;
IF : 'i' 'f';
ELSE : 'e' 'l' 's' 'e';
WHILE : 'w' 'h' 'i' 'l' 'e';
FOR : 'f' 'o' 'r';
FUNCTION: 'f' 'u' 'n' 'c' 't' 'i' 'o' 'n';

ASSIGNMENT_OPERATOR: '+=' | '-=' | '*=' | '/=' | '=';

ADDITIVE_OP: '+' | '-';
MULTIPLICATIVE_OP: '*' | '/';
CARAT: '^';

LOW_PRIORITY_OP: MOD_OPERATOR;
COMPARE_OP: EQUALITY_OPERATOR
		  | GREATER
		  | LESSER
		  | GREATER_STRICT
		  | LESSER_STRICT;
LOGICAL_OP: '&&' | '||';

BANG: '!';

fragment CHARACTER : 'a'..'z'|'A'..'Z';
IDENTIFIER: (CHARACTER | '_') (CHARACTER | DIGIT | '_')*;

L_PARENTHESIS : '(';
R_PARENTHESIS : ')';
L_BRACE : '{';
R_BRACE : '}';

UNKNOWN: .;