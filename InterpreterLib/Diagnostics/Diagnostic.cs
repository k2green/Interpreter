using InterpreterLib.Binding;
using InterpreterLib.Binding.Types;
using System;

namespace InterpreterLib {
	public class Diagnostic {

		public string Message { get; }
		public int Line { get; }
		public int Column { get; }

		internal Diagnostic(int line, int column, string message) {
			Message = $"line {line}:{column}\t{message}";
			Line = line;
			Column = column;
		}

		public override string ToString() => Message;

		internal static Diagnostic ReportInvalidLiteral(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid literal syntax {v}");
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid binary expression syntax {v}");
		}

		internal static Diagnostic ReportMissingTypeDelimeter(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Missing type delimeter in {v}");
		}

		internal static Diagnostic ReportMissingTypeName(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Missing type name in {v}");
		}

		internal static Diagnostic ReportMissingExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Missing expression in {v}");
		}

		internal static Diagnostic ReportMissingOperator(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Missing operator in {v}");
		}

		internal static Diagnostic ReportMissingIdentifier(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Missing identifier in {v}");
		}

		internal static Diagnostic ReportInvalidTypeDefinition(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid type definition syntax {v}");
		}

		internal static Diagnostic ReportInvalidAssignmentOperand(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid type operand {v}");
		}

		internal static Diagnostic ReportInvalidDeclarationKeyword(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid keyword {v}. Variable declarations must begin with 'var' or 'val'");
		}

		internal static Diagnostic ReportMalformedDeclaration(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid declaration syntax {v}");
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid binary expression {v}");
		}

		internal static Diagnostic ReportInvalidExpressionStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid expression statement {v}");
		}

		internal static Diagnostic ReportInvalidElseClase(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid else clause {v}");
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid if statement {v}");
		}

		internal static Diagnostic ReportInvalidForStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid for statement {v}");
		}

		internal static Diagnostic ReportInvalidForAssignment(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid for statement assignment {v}");
		}

		internal static Diagnostic ReportUnknownDeclKeyword(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax error: Invalid declaration keword {v}. Variable declarations must start with 'var' or 'val'");
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, string opText, TypeSymbol leftType, TypeSymbol rightType) {
			return new Diagnostic(line, column, $"Operator {opText} is invalid for types {leftType} and {rightType}");
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, string opText, TypeSymbol valueType) {
			return new Diagnostic(line, column, $"Operator {opText} is invalid for type {valueType}");
		}

		internal static Diagnostic ReportAssingmentTypeDef(int line, int column, string v) {
			return new Diagnostic(line, column, $"Assignment operations cannot have a type definition {v}");
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, string varaibleName) {
			return new Diagnostic(line, column, $"The variable {varaibleName} does not exist");
		}

		internal static Diagnostic ReportInvalidType(int line, int column, string v, TypeSymbol expected) {
			return new Diagnostic(line, column, $"The expression {v} is not of type {expected}");
		}

		internal static Diagnostic ReportFailedBind(int line, int column, string v) {
			return new Diagnostic(line, column, $"Failed to bind {v}");
		}

		internal static Diagnostic ReportUnknownTypeKeyword(int line, int column, string v) {
			return new Diagnostic(line, column, $"Unknown type {v}");
		}

		internal static Diagnostic ReportCannotCast(int line, int column, TypeSymbol actual, TypeSymbol expected) {
			return new Diagnostic(line, column, $"Cannot implicitly convert {actual} to {expected}");
		}

		internal static Diagnostic ReportVariableTypeMismatch(int line, int column, string identifierText, TypeSymbol varType, TypeSymbol exprType) {
			return new Diagnostic(line, column, $"Cannot reassign {identifierText} : {varType} to {exprType}");
		}

		internal static Diagnostic ReportCannotRedefine(int line, int column, string identifierText) {
			return new Diagnostic(line, column, $"The variable {identifierText} already exits");
		}

		internal static Diagnostic ReportInvalidBody(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid body {v}");
		}

		internal static Diagnostic ReportInvalidStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid Statement {v}");
		}

		internal static Diagnostic ReportInvalidFunctionCall(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid function call {v}");
		}

		internal static Diagnostic ReportInvalidParameters(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid paremeter list {v}");
		}

		internal static Diagnostic ReportInvalidParameter(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid paremeter {v}");
		}

		internal static Diagnostic ReportSyntaxError(int line, int column, string v) {
			return new Diagnostic(line, column, $"Syntax Error: {v}");
		}
	}
}
