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

		internal static Diagnostic ReportInvalidLiteral(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid literal syntax {text}");
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid binary expression syntax {text}");
		}

		internal static Diagnostic ReportMissingTypeDelimeter(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Missing type delimeter in {text}");
		}

		internal static Diagnostic ReportErrorEncounteredWhileEvaluating() {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportMissingTypeName(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Missing type name in {text}");
		}

		internal static Diagnostic ReportMissingExpression(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Missing expression in {text}");
		}

		internal static Diagnostic ReportMissingOperator(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Missing operator in {text}");
		}

		internal static Diagnostic ReportMissingIdentifier(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Missing identifier in {text}");
		}

		internal static Diagnostic ReportInvalidTypeDefinition(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid type definition syntax {text}");
		}

		internal static Diagnostic ReportInvalidAssignmentOperand(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid type operand {text}");
		}

		internal static Diagnostic ReportInvalidDeclarationKeyword(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid keyword {text}. Variable declarations must begin with 'var' or 'val'");
		}

		internal static Diagnostic ReportMalformedDeclaration(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid declaration syntax {text}");
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid binary expression {text}");
		}

		internal static Diagnostic ReportInvalidExpressionStatement(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid expression statement {text}");
		}

		internal static Diagnostic ReportInvalidElseClase(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid else clause {text}");
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid if statement {text}");
		}

		internal static Diagnostic ReportInvalidForStatement(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid for statement {text}");
		}

		internal static Diagnostic ReportInvalidForAssignment(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid for statement assignment {text}");
		}

		internal static Diagnostic ReportUnknownDeclKeyword(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax error: Invalid declaration keword {text}. Variable declarations must start with 'var' or 'val'");
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, string opText, TypeSymbol leftType, TypeSymbol rightType) {
			var leftTypeString = leftType == null ? "<Null>" : leftType.ToString();
			var rightTypeString = rightType == null ? "<Null>" : rightType.ToString();

			return new Diagnostic(line, column, $"Operator {opText} is invalid for types {leftTypeString} and {rightTypeString}");
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, string opText, TypeSymbol valueType) {
			var valueTypeString = valueType == null ? "<Null>" : valueType.ToString();

			return new Diagnostic(line, column, $"Operator {opText} is invalid for type {valueTypeString}");
		}

		internal static Diagnostic ReportAssingmentTypeDef(int line, int column, string text) {
			return new Diagnostic(line, column, $"Assignment operations cannot have a type definition {text}");
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, string varaibleName) {
			return new Diagnostic(line, column, $"The variable {varaibleName} does not exist");
		}

		internal static Diagnostic ReportInvalidType(int line, int column, string text, TypeSymbol expected) {
			return new Diagnostic(line, column, $"The expression {text} is not of type {expected}");
		}

		internal static Diagnostic ReportFailedBind(int line, int column, string text) {
			return new Diagnostic(line, column, $"Failed to bind {text}");
		}

		internal static Diagnostic ReportUnknownTypeKeyword(int line, int column, string text) {
			return new Diagnostic(line, column, $"Unknown type {text}");
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

		internal static Diagnostic ReportInvalidBody(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid body {text}");
		}

		internal static Diagnostic ReportFailedVisit(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid Statement {text}");
		}

		internal static Diagnostic ReportInvalidFunctionCall(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid function call {text}");
		}

		internal static Diagnostic ReportInvalidParameters(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid paremeter list {text}");
		}

		internal static Diagnostic ReportInvalidParameter(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid paremeter {text}");
		}

		internal static Diagnostic ReportSyntaxError(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: {text}");
		}

		internal static Diagnostic ReportInvalidTypedIdentifier(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid function declaration {text}");
		}

		internal static Diagnostic ReportInvalidParameterDefinition(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid parameter declaration {text}");
		}

		internal static Diagnostic ReportInvalidFunctionDef(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Invalid function declaration {text}");
		}

		internal static Diagnostic ReportInvalidCallParameters(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Parameter list \"{text}\" is invalid. Are you missing a comma?");
		}

		internal static Diagnostic ReportMissingComma(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: Missing comma at {text}");
		}

		internal static Diagnostic ReportInvalidCallParameter(int line, int column, string text) {
			return new Diagnostic(line, column, $"Syntax Error: {text}");
		}
	}
}
