grammar GLang;

/*
 * Parser Rules
 */
 
literal : INTEGER | BOOLEAN | IDENTIFIER;

block : L_BRACE statement* R_BRACE;

forStat: FOR L_PARENTHESIS assign=assignmentStatement COMMA condition=binaryExpression COMMA step=assignmentStatement R_PARENTHESIS body=statement;

ifStat : IF L_PARENTHESIS condition=binaryExpression R_PARENTHESIS trueBranch=statement (ELSE falseBranch=statement)?;
whileStat: WHILE L_PARENTHESIS condition=binaryExpression R_PARENTHESIS body=statement;

statement :  forStat | whileStat | ifStat | block| assignmentStatement | variableDeclaration  | binaryExpression;

variableDeclaration : DECL_VARIABLE IDENTIFIER (TYPE_DELIMETER TYPE_NAME)?;
assignmentStatement : variableDeclaration ASSIGNMENT_OPERATOR expr=binaryExpression
					| IDENTIFIER ASSIGNMENT_OPERATOR expr=binaryExpression;

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
				
/*
 * Lexer Rules
 */

fragment DIGITS : '0'..'9'+;
fragment TRUE : ('T' | 't') 'rue';
fragment FALSE : ('F' | 'f') 'alse';
fragment CHARACTER : 'a'..'z'|'A'..'Z';

fragment VAR: 'var';
fragment VAL: 'val';

fragment INT: 'int';
fragment BOOL: 'bool';

fragment EQUALITY_OPERATOR: '==';
fragment GREATER: '>=';
fragment LESSER: '<=';
fragment GREATER_STRICT: '>';
fragment LESSER_STRICT: '<';

fragment MOD_OPERATOR : 'mod';

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);

COMMA : ',';

TYPE_DELIMETER : ':';
TYPE_NAME : INT | BOOL;

BOOLEAN: TRUE | FALSE;
INTEGER: DIGITS;

DECL_VARIABLE : VAL | VAR;
IF : 'if';
ELSE : 'else';
WHILE : 'while';
FOR : 'for';

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

IDENTIFIER: CHARACTER (CHARACTER | '_')*;

L_PARENTHESIS : '(';
R_PARENTHESIS : ')';
L_BRACE : '{';
R_BRACE : '}';

UNKNOWN_CHAR: .;