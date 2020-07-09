using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Syntax;
using InterpreterLib.Types;
using System;

namespace InterpreterLib {
	public class Diagnostic {

		public string Message { get; }
		public TextSpan? PreviousText { get; }
		public TextSpan? OffendingText { get; }
		public TextSpan? NextText { get; }
		public int Line { get; }
		public int Column { get; }

		internal Diagnostic(int line, int column, string message, TextSpan? previousText, TextSpan? offendingText, TextSpan? nextText) {
			Line = line;
			Column = column;
			Message = message;
			PreviousText = previousText;
			OffendingText = offendingText;
			NextText = nextText;
		}

		public override string ToString() => Message;

		internal static Diagnostic ReportInvalidLiteral(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid literal syntax", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, TextSpan? previousText, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid unary expression syntax", previousText, offendingText, null);
		}

		internal static Diagnostic ReportErrorEncounteredWhileEvaluating() {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, TextSpan offendingText) {
			return ReportInvalidUnaryExpression(line, column, null, offendingText);
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid binary expression syntax", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, TextSpan offendingText) {
			return ReportInvalidBinaryExpression(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportMissingToken(int line, int column, TextSpan offendingText, string tokenName) {
			return new Diagnostic(line, column, $"Syntax error: Missing {tokenName}", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidTypeDefinition(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid type definition", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidAssignmentOperand(int line, int column, TextSpan previousText, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid assignment operand", previousText, offendingText, null);
		}

		internal static Diagnostic ReportMalformedDeclaration(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid declaration statement", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidDeclarationKeyword(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid declaration keyword", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidExpressionStatement(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid expression statement", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidInternalForAssignment(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid for assignment step", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid if statement", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, TextSpan offendingText) {
			return ReportInvalidIfStatement(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidWhileStatement(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid while statement", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidWhileStatement(int line, int column, TextSpan offendingText) {
			return ReportInvalidWhileStatement(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidForStatement(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid for statement", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidForStatement(int line, int column, TextSpan offendingText) {
			return ReportInvalidForStatement(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidBlock(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid body ", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidStatement(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid statement ", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidFunctionCall(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid Function call ", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidCallParameters(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid parameters", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidCallParameter(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid parameter", null, offendingText, null);
		}

		internal static Diagnostic ReportMissingComma(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Missing comma", previousText, offendingText, previousText);
		}

		internal static Diagnostic ReportSyntaxError(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: ", null, offendingText, null);
		}

		internal static Diagnostic ReportUndefinedFunction(int line, int column, TextSpan span) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidCompilationUnit(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid compilation unit", null, offendingText, null);
		}

		internal static Diagnostic ReportFunctionCountMismatch(int line, int column, int syntaxCount, int requiredCount, TextSpan span) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidGlobalStatement(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid global statement", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidTypedIdentifier(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid typed identifier", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidParameterDefinition(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid typed parameter", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidParameterType(int line, int column, TypeSymbol valueType, TypeSymbol requiredType, TextSpan span) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidFunctionDef(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid function definition", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidFunctionDef(int line, int column, TextSpan offendingText) {
			return ReportInvalidFunctionDef(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidType(int line, int column, TextSpan previousText, TextSpan offendingText, TextSpan nextText, TypeSymbol requiredType) {
			return new Diagnostic(line, column, $"Invalid type. Required type: {requiredType}", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidAssignmentTypeDef(int line, int column, TextSpan previousText, TextSpan offendingText) {
			return new Diagnostic(line, column, "Variable types cannot be redefined", previousText, offendingText, null);
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Variable is undefined", null, offendingText, null);
		}

		internal static Diagnostic ReportUnknownDeclKeyword(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Unknown variable declaration keyeword", null, offendingText, null);
		}

		internal static Diagnostic ReportUnknownTypeKeyword(int line, int column, TextSpan prevText, TextSpan offendingText) {
			return new Diagnostic(line, column, "Unknown variable type", prevText, offendingText, null);
		}

		internal static Diagnostic ReportCannotCast(int line, int column, TextSpan prevText, TextSpan offendingText, TypeSymbol fromType, TypeSymbol toType) {
			return new Diagnostic(line, column, $"Cannot implicitly convert {fromType} to {toType}", prevText, offendingText, null);
		}

		internal static Diagnostic ReportVoidType(int line, int column, TextSpan? prevText, TextSpan offendingText) {
			return new Diagnostic(line, column, "Invalid use of void type", prevText, offendingText, null);
		}

		internal static Diagnostic ReportVoidType(int line, int column, TextSpan offendingText) {
			return ReportVoidType(line, column, null, offendingText);
		}

		internal static Diagnostic ReportCannotRedefine(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Cannot redefine vadriable", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, TextSpan prevText, TextSpan offendingText, TextSpan nextText, TypeSymbol valueType1, TypeSymbol valueType2) {
			return new Diagnostic(line, column, $"Operator is invalid for types {valueType1} and {valueType2}", prevText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidOperator(int line, int column, TextSpan prevText, TextSpan offendingText, TypeSymbol valueType) {
			return new Diagnostic(line, column, $"Operator is invalid for type {valueType}", prevText, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnType(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, $"Invalid return type", null, offendingText, null);
		}

		internal static Diagnostic ReportCannotRedefineFunction(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, $"Cannot redefine function", null, offendingText, null);
		}
	}
}
