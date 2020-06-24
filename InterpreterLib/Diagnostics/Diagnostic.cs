using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

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

		internal static Diagnostic ReportInvalidUnaryOperator(IToken opToken, Type operandType) {
			return new Diagnostic(opToken.Line, opToken.Column, $"'{opToken.Text}' Operator is invalid for type {operandType}");
		}

		internal static Diagnostic ReportInvalidBinaryOperator(IToken opToken, Type leftType, Type rightType) {
			return new Diagnostic(opToken.Line, opToken.Column, $"'{opToken.Text}' Operator is invalid for types {leftType} and {rightType}");
		}

		internal static Diagnostic ReportInvalidSyntax(IToken offendingSymbol, string invalidText) {
			return new Diagnostic(offendingSymbol.Line, offendingSymbol.Column, $"Invalid Syntax: {invalidText}");
		}

		internal static Diagnostic ReportUndefinedVariable(IToken offendingSymbol) {
			return new Diagnostic(offendingSymbol.Line, offendingSymbol.Column, $"Variable {offendingSymbol.Text} is undefined");
		}

		internal static Diagnostic ReportRedefineVariable(IToken offendingSymbol) {
			return new Diagnostic(offendingSymbol.Line, offendingSymbol.Column, $"Variable cannot be reassigned {offendingSymbol.Text}");
		}

		internal static Diagnostic ReportInvelidLiteral(int line, int column, string text) {
			return new Diagnostic(line, column, $"Invalid Literal {text}");
		}

		internal static Diagnostic ReportInvalidStatement(IToken start, string v) {
			return new Diagnostic(start.Line, start.Column, $"Invalid Statement {v}");
		}

		internal static Diagnostic ReportInvalidDeclaration(IToken start, string v) {
			return new Diagnostic(start.Line, start.Column, $"Invalid variable declaration {v}");
		}

		internal static Diagnostic ReportTypeMismatch(IToken start, Type valueType1, Type valueType2) {
			return new Diagnostic(start.Line, start.Column, $"Type mismatch between {valueType1} and {valueType2}");
		}
	}
}
