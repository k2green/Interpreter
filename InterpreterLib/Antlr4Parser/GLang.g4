grammar GLang;

/*
 * Parser Rules
 */
 
compilationUnit : globalStatement+;

declerationOrAssign: variableDeclarationStatement | assignmentExpression;
typeDefinition: TYPE_DELIMETER typeDescription;

tupleDescription : L_PARENTHESIS seperatedTypeDescription R_PARENTHESIS;
seperatedTypeDescription : typeDescription COMMA seperatedTypeDescription | typeDescription;
typeDescription	: tupleDescription
				| TYPE_NAME;

	/*
	 * Statements
	 */

	
globalStatement: functionDefinition | baseStatement;
baseStatement : forStatement | whileStatement | ifStatement | block | variableDeclarationStatement | expressionStatement;
statement : returnStatement | baseStatement | BREAK | CONTINUE;

definedIdentifier: IDENTIFIER typeDefinition;
seperatedDefinedIdentifier: definedIdentifier COMMA;

returnStatement : RETURN expression
				| RETURN;

parameterDefinition	: definedIdentifier COMMA parameterDefinition
					| definedIdentifier;

functionDefinition: FUNCTION IDENTIFIER L_PARENTHESIS parameterDefinition R_PARENTHESIS typeDefinition block
				  | FUNCTION IDENTIFIER L_PARENTHESIS R_PARENTHESIS typeDefinition block;

block : L_BRACE statement* R_BRACE;

expressionStatement: assignmentExpression | expression;

forStatement: FOR L_PARENTHESIS declerationOrAssign COMMA binaryExpression COMMA assignmentExpression R_PARENTHESIS statement;

ifStatement: ifElseStatement | pureIfStatement;
pureIfStatement : IF L_PARENTHESIS binaryExpression R_PARENTHESIS trueBranch=statement;
ifElseStatement : pureIfStatement ELSE falseBranch=statement;

whileStatement: WHILE L_PARENTHESIS binaryExpression R_PARENTHESIS body=statement;

variableDeclarationStatement: DECL_VARIABLE definedIdentifier
							| DECL_VARIABLE assignmentExpression;

assignmentOperand : expression;
assignmentExpression : baseAssignmentExpression;

baseAssignmentExpression : IDENTIFIER ASSIGNMENT_OPERATOR assignmentOperand;
tupleAssignmentExpression : L_PARENTHESIS seperatedIdentifier

seperatedIdentifier : seperatedIdentifierAtom COMMA seperatedIdentifier | seperatedIdentifierAtom;
seperatedIdentifierAtom : IDENTIFIER | WILDCARD;

	/*
	 * Expressions
	 */
	 

literal : DOUBLE | INTEGER | BOOLEAN | accessorExpression | STRING | CHAR_LITERAL | BYTE;
expression: tuple | binaryExpression;

indexedIdentifier: IDENTIFIER L_BRACKET binaryExpression R_BRACKET;

accessorExpression: accessorAtom DOT accessorExpression
				  | accessorAtom;
				  
accessorAtom: assignmentExpression
			| indexedIdentifier
			| functionCall
			| IDENTIFIER;

tuple : L_PARENTHESIS seperatedExpression R_PARENTHESIS;

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
				

seperatedExpression	: expression COMMA seperatedExpression
					| expression;

functionCall: funcName=IDENTIFIER L_PARENTHESIS seperatedExpression R_PARENTHESIS
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
fragment CHARACTER_TYPE: 'char';
fragment BYTE_TYPE: 'byte';
fragment VOID_TYPE: 'void';

fragment EQUALITY_OPERATOR: '==' | '!=';
fragment GREATER: '>=';
fragment LESSER: '<=';
fragment GREATER_STRICT: '>';
fragment LESSER_STRICT: '<';

fragment MOD_OPERATOR : 'mod';

STRING : '"' .*? '"';
CHAR_LITERAL: '\'' . '\'';

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);

COMMA : ',';
WILDCARD : '_';

TYPE_DELIMETER : ':';
BREAK: 'b' 'r' 'e' 'a' 'k';
CONTINUE : 'c' 'o' 'n' 't' 'i' 'n' 'u' 'e';
RETURN : 'r' 'e' 't' 'u' 'r' 'n' ;
TYPE_NAME : INTEGER_TYPE | DOUBLE_TYPE | BOOLEAN_TYPE | STRING_TYPE | VOID_TYPE | CHARACTER_TYPE | BYTE_TYPE;

BOOLEAN: TRUE | FALSE;
DOUBLE: '.' DIGIT+ | DIGIT+ '.' DIGIT+;
BYTE : DIGIT+ ('b' | 'B');
INTEGER: DIGIT+;

DOT : '.';

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
L_BRACKET : '[';
R_BRACKET : ']';

UNKNOWN: .;