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
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidIfStatement(int line, int column, string v) {
			throw new NotImplementedException();
		}
	}
}
