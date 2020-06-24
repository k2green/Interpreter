using InterpreterLib.Binding;
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

		internal static Diagnostic ReportInvalidUnaryOperator(int line, int column, string operatorText, Type operandType) {
			return new Diagnostic(line, column, $"'{operatorText}' Operator is invalid for type {operandType}");
		}

		internal static Diagnostic ReportInvalidBinaryOperator(int line, int column, string operatorText, Type leftType, Type rightType) {
			return new Diagnostic(line, column, $"'{operatorText}' Operator is invalid for types {leftType} and {rightType}");
		}

		internal static Diagnostic ReportInvalidSyntax(int line, int column, string invalidText) {
			return new Diagnostic(line, column, $"Invalid Syntax {invalidText}");
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, BoundVariable variable) {
			return ReportUndefinedVariable(line, column, variable.ToString());
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, string message) {
			return new Diagnostic(line, column, $"Variable {message} is undefined");
		}

		internal static Diagnostic ReportRedefineVariable(int line, int column, BoundVariable variable, BoundVariable redefine) {
			return new Diagnostic(line, column, $"Variable {variable} cannot be reassigned {redefine}");
		}

		internal static Diagnostic ReportInvelidLiteral(int line, int column, string text) {
			return new Diagnostic(line, column, $"Invalid Literal {text}");
		}

		internal static Diagnostic ReportInvalidStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Invalid Statement {v}");
		}

		internal static Diagnostic ReportInvalidDeclaration(int line, int column, string v) {
			return new Diagnostic(line, column, $"Invalid variable declaration {v}");
		}

		internal static Diagnostic ReportVariableTypeMismatch(int line, int column, string name, Type valueType1, Type valueType2) {
			return new Diagnostic(line, column, $"Cannot reassign {name} from {valueType1} to {valueType2}");
		}

		internal static Diagnostic ReportReadonlyVariable(int line, int column, BoundVariable lookup) {
			return new Diagnostic(line, column, $"Variable {lookup} is read only");
		}
	}
}
