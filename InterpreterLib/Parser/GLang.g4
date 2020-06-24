grammar GLang;

/*
 * Parser Rules
 */
 
literal : DOUBLE | INTEGER | BOOLEAN | IDENTIFIER | STRING;

block : L_BRACE statement* R_BRACE;

ifStat : IF L_BRACKET condition=binaryExpression R_BRACKET trueBranch=statement ELSE falseBranch=statement;

statement : ifStat | block | expression;

expression : assignmentExpression | binaryExpression;

assignmentExpression : decl=DECL_VARIABLE? IDENTIFIER ASSIGNMENT_OPERATOR expr=binaryExpression;

unaryExpression : L_BRACKET binaryExpression R_BRACKET
				| op=(ADDITIVE_OP | BANG) unaryExpression
				| atom=literal;

binaryExpression: left=binaryExpression op=CARAT right=binaryExpression
				| left=binaryExpression op=MULTIPLICATIVE_OP right=binaryExpression
				| left=binaryExpression op=ADDITIVE_OP right=binaryExpression
				| left=binaryExpression op=LOW_PRIORITY_OP right=binaryExpression
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

fragment EQUALITY_OPERATOR: '==';
fragment LOGICAL_OPERATOR: '&&' | '||';
fragment MOD_OPERATOR : 'mod';

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);

BOOLEAN: TRUE | FALSE;
DOUBLE: DIGITS? '.' DIGITS;
INTEGER: DIGITS;
STRING: '"' .*? '"';

DECL_VARIABLE : VAL | VAR;
IF : 'if';
ELSE : 'else';

ADDITIVE_OP: '+' | '-';
MULTIPLICATIVE_OP: '*' | '/';
LOW_PRIORITY_OP: MOD_OPERATOR | EQUALITY_OPERATOR | LOGICAL_OPERATOR;
CARAT: '^';
ASSIGNMENT_OPERATOR: '=';
BANG: '!';

IDENTIFIER: CHARACTER (CHARACTER | '_')*;

L_BRACKET : '(';
R_BRACKET : ')';
L_BRACE : '{';
R_BRACE : '}';

UNKNOWN_CHAR: .;