grammar GLang;

/*
 * Parser Rules
 */
 
literal : INTEGER | BOOLEAN | IDENTIFIER;

block : L_BRACE statements=statement* R_BRACE;

statement : assignmentExpression | binaryExpression;

assignmentExpression : decl=DECL_VARIABLE? IDENTIFIER ASSIGNMENT_OPERATOR expression=binaryExpression;

unaryExpression : L_BRACKET binaryExpression R_BRACKET
				| op=(ADDITIVE_OP | BANG) unaryExpression
				| atom=literal;

binaryExpression: left=binaryExpression op=CARAT right=binaryExpression
				| left=binaryExpression op=MULTIPLICATIVE_OP right=binaryExpression
				| left=binaryExpression op=ADDITIVE_OP right=binaryExpression
				| left=binaryExpression op=MOD_OPERATOR right=binaryExpression
				| left=binaryExpression op=LOGICAL_OPERATOR right=binaryExpression
				| left=binaryExpression op=EQUALITY_OPERATOR right=binaryExpression
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

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);

INTEGER: DIGITS;
BOOLEAN: TRUE | FALSE;

DECL_VARIABLE : VAL | VAR;

MOD_OPERATOR : 'mod';
ADDITIVE_OP: '+' | '-';
MULTIPLICATIVE_OP: '*' | '/';
CARAT: '^';
EQUALITY_OPERATOR: '==';
ASSIGNMENT_OPERATOR: '=';
LOGICAL_OPERATOR: '&&' | '||';
BANG: '!';

IDENTIFIER: CHARACTER (CHARACTER | '_')*;

L_BRACKET : '(';
R_BRACKET : ')';
L_BRACE : '{';
R_BRACE : '}';

UNKNOWN_CHAR: .;