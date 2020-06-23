using Antlr4.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib {
	internal class Diagnostic : IEnumerable<string> {

		private List<string> messages;

		public Diagnostic() {
			messages = new List<string>();
		}

		private void ReportError(int line, int column, string message) {
			messages.Add($"line {line}:{column}\t{message}");
		}

		public void ReportInvalidUnaryOperator(IToken opToken, Type operandType) {
			ReportError(opToken.Line, opToken.Column, $"'{opToken.Text}' Operator is invalid for type {operandType}");
		}

		public void ReportInvalidBinaryOperator(IToken opToken, Type leftType, Type rightType) {
			ReportError(opToken.Line, opToken.Column, $"'{opToken.Text}' Operator is invalid for types {leftType} and {rightType}");
		}

		public void ReportInvalidSyntax(IToken offendingSymbol, string invalidText) {
			ReportError(offendingSymbol.Line, offendingSymbol.Column, $"Invalid Syntax: {invalidText}");
		}

		public void ReportUndefinedVariable(IToken offendingSymbol) {
			ReportError(offendingSymbol.Line, offendingSymbol.Column, $"Variable {offendingSymbol.Text} is undefined");
		}

		public void ReportRedefineVariable(IToken offendingSymbol) {
			ReportError(offendingSymbol.Line, offendingSymbol.Column, $"Variable cannot be reassigned {offendingSymbol.Text}");
		}

		public void ReportInvelidLiteral(int line, int column, string text) {
			ReportError(line, column, $"Invalid Literal {text}");
		}

		public IEnumerator<string> GetEnumerator() {
			return messages.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		internal void ReportInvalidStatement(IToken start, string v) {
			ReportError(start.Line, start.Column, $"Invalid Statement {v}");
		}

		internal void ReportInvalidDeclaration(IToken start, string v) {
			ReportError(start.Line, start.Column, $"Invalid variable declaration {v}");
		}

		internal void ReportTypeMismatch(IToken start, Type valueType1, Type valueType2) {
			ReportError(start.Line, start.Column, $"Type mismatch between {valueType1} and {valueType2}");
		}
	}
}
