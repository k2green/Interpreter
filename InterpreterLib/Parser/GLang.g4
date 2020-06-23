grammar GLang;

/*
 * Parser Rules
 */
 
 literal: INTEGER {DataType type = DataType.Intiger;};

/*
 * Lexer Rules
 */

fragment DIGITS : '0'..'9'+;

WHITESPACE : [ \t\r\n]+ -> channel(HIDDEN);
INTEGER : DIGITS;
ADDITIVE_OPERATOR : '+' | '-';
MULTIPLICATIVE_OPERATOR : '*' | '/';
CARAT_OPERATOR : '^';