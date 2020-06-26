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

		internal static Diagnostic ReportInvalidUnaryOperator(int line, int column, string operatorText, TypeSymbol operandType) {
			return new Diagnostic(line, column, $"'{operatorText}' Operator is invalid for type {operandType}");
		}

		internal static Diagnostic ReportInvalidBinaryOperator(int line, int column, string operatorText, TypeSymbol leftType, TypeSymbol rightType) {
			return new Diagnostic(line, column, $"'{operatorText}' Operator is invalid for types {leftType} and {rightType}");
		}

		internal static Diagnostic ReportInvalidSyntax(int line, int column, string invalidText) {
			return new Diagnostic(line, column, $"Invalid Syntax {invalidText}");
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, VariableSymbol variable) {
			return ReportUndefinedVariable(line, column, variable.ToString());
		}

		internal static Diagnostic ReportUndefinedVariable(int line, int column, string message) {
			return new Diagnostic(line, column, $"Variable {message} is undefined");
		}

		internal static Diagnostic ReportRedefineVariable(int line, int column, VariableSymbol variable, VariableSymbol redefine) {
			return new Diagnostic(line, column, $"Variable {variable} cannot be reassigned {redefine}");
		}

		internal static Diagnostic ReportInvelidLiteral(int line, int column, string text) {
			return new Diagnostic(line, column, $"Invalid Literal {text}");
		}

		internal static Diagnostic ReportInvalidExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"Invalid Statement {v}");
		}

		internal static Diagnostic ReportInvalidDeclaration(int line, int column, string v) {
			return new Diagnostic(line, column, $"Invalid variable declaration {v}");
		}

		internal static Diagnostic ReportInvalidLiteral(int line, int column, string v) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportVariableTypeMismatch(int line, int column, string name, TypeSymbol valueType1, TypeSymbol valueType2) {
			return new Diagnostic(line, column, $"Cannot reassign {name} from {valueType1} to {valueType2}");
		}

		internal static Diagnostic ReportReadonlyVariable(int line, int column, VariableSymbol lookup) {
			return new Diagnostic(line, column, $"Variable {lookup} is read only");
		}

		internal static Diagnostic ReportInvalidStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"Statemment {v} is invalid");
		}

		internal static Diagnostic ReportInvalidWhile(int line, int column, string v) {
			return new Diagnostic(line, column, $"While loop {v} is invalid");
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, string v) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidTypeName(int line, int column, string v) {
			return new Diagnostic(line, column, $"{v} is not a valid type");
		}

		internal static Diagnostic ReportInvalidAssignment(int line, int column, string v) {
			return new Diagnostic(line, column, $"Assignment {v} is invalid");
		}

		internal static Diagnostic ReportInvalidFor(int line, int column, string v) {
			return new Diagnostic(line, column, $"For loop {v} is invalid");
		}

		internal static Diagnostic ReportInvalidType(int line, int column, TypeSymbol valueType, TypeSymbol boolean) {
			return new Diagnostic(line, column, $"Invalid type: {valueType} is not {boolean}");
		}

		internal static Diagnostic ReportInvalidElse(int line, int column, string v) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, string v) {
			throw new NotImplementedException();
		}
	}
}
