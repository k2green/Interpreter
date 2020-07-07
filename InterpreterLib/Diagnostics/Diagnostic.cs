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

		internal static Diagnostic ReportInvalidTypeDefinition(int line, int column, TextSpan previousText, TextSpan offendingText, TextSpan nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid TypeDefinitionSyntax", previousText, offendingText, nextText);
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

		internal static Diagnostic ReportInvalidInternalForAssignment(int line, int column, TextSpan offendingText, TextSpan nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid for assignment step", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid if statement", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, TextSpan offendingText) {
			return ReportInvalidIfStatement(line, column, null, offendingText, null);
		}
	}
}
