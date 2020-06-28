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

		internal static Diagnostic ReportUndefinedVariable(int line, int column, string v) {
			return new Diagnostic(line, column, $"Variable name {v} does not exist");
		}

		internal static Diagnostic ReportInvalidLiteral(int line, int column, string v) {
			return new Diagnostic(line, column, $"The literal {v} is invalid");
		}

		internal static Diagnostic ReportInvalidUnaryOperator(int line, int column, string text, TypeSymbol valueType) {
			return new Diagnostic(line, column, $"The unary operator {text} is invalid for type {valueType}");
		}

		internal static Diagnostic ReportInvalidUnaryExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"The unary expression {v} is invalid");
		}

		internal static Diagnostic ReportInvalidBinaryOperator(int line, int column, string text, TypeSymbol valueType1, TypeSymbol valueType2) {
			return new Diagnostic(line, column, $"The unary operator {text} is invalid for types {valueType1} and {valueType2}");
		}

		internal static Diagnostic ReportInvalidBinaryExpression(int line, int column, string v) {
			return new Diagnostic(line, column, $"The binary expression {v} is invalid");
		}

		internal static Diagnostic ReportInvalidAssignment(int line, int column, string v) {
			return new Diagnostic(line, column, $"The assignment {v} is invalid");
		}

		internal static Diagnostic ReportReadonlyVariable(int line, int column, VariableSymbol variable) {
			return new Diagnostic(line, column, $"The variable {variable} is read only");
		}

		internal static Diagnostic ReportVariableTypeMismatch(int line, int column, string name, TypeSymbol valueType1, TypeSymbol valueType2) {
			return new Diagnostic(line, column, $"Cannot assign {name} from {valueType1} to {valueType2}");
		}

		internal static Diagnostic ReportFailedVisit(int line, int column, string v) {
			return new Diagnostic(line, column, $"Failed to visit {v}");
		}

		internal static Diagnostic ReportInvalidDeclaration(int line, int column, string v) {
			return new Diagnostic(line, column, $"Invalid declaration caused by {v}");
		}

		internal static Diagnostic ReportRedefineVariable(int line, int column, VariableSymbol variableSymbol, VariableSymbol variable) {
			return new Diagnostic(line, column, $"Cannot redefine {variableSymbol} to {variable}");
		}

		internal static Diagnostic ReportInvalidStatement(int line, int column, string v) {
			return new Diagnostic(line, column, $"The statament {v} is invalid");
		}

		internal static Diagnostic ReportInvalidBlock(int line, int column, string v) {
			return new Diagnostic(line, column, $"The block {v} is invalid");
		}

		internal static Diagnostic ReportInvalidFor(int line, int column, string v) {
			return new Diagnostic(line, column, $"The for loop {v} is invalid");
		}

		internal static Diagnostic ReportInvalidType(int line, int column, TypeSymbol valueType, TypeSymbol desiredType) {
			return new Diagnostic(line, column, $"The type {valueType} does not match type {desiredType}");
		}

		internal static Diagnostic ReportInvalidIf(int line, int column, string v) {
			return new Diagnostic(line, column, $"The if {v} is invalid");
		}

		internal static Diagnostic ReportMalformedElse(int line, int column, string v) {
			return new Diagnostic(line, column, $"The else clause {v} is invalid");
		}

		internal static Diagnostic ReportInvalidWhile(int line, int column, string text) {
			return new Diagnostic(line, column, $"The while loop {text} is invalid");
		}

		internal static Diagnostic ReportInvalidDeclarationKeyword(int line, int column, string text) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportMissingTypeDelimeter(int line, int column) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportMissingTypeName(int line, int column) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidTypeName(int line, int column, string text) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportCannotDefine(int line, int column, string name) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportCannotRedefine(int line, int column, string name, TypeSymbol valueType1, TypeSymbol valueType2) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidExpressionStatement(int line, int column, string text) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportCannotCast(int line, int column, string v) {
			throw new NotImplementedException();
		}
	}
}
