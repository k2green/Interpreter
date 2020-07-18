using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Symbols.Types;
using InterpreterLib.Syntax;
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

		internal static Diagnostic ReportInvalidAccessor(int line, int column, TextSpan span) {
			throw new NotImplementedException();
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

		internal static Diagnostic ReportInvalidIndexer(int line, int column, TextSpan span) {
			throw new NotImplementedException();
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

		internal static Diagnostic ReportUndefinedFunction(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Function is undefined", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidCompilationUnit(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, "Syntax error: Invalid compilation unit", null, offendingText, null);
		}

		internal static Diagnostic ReportFunctionCountMismatch(TextLocation location, int syntaxCount, int requiredCount, TextSpan offendingText) {
			var reqString = $"{requiredCount} parameter{(requiredCount == 1 ? "" : "s")}";
			var countString = $"{syntaxCount} parameter{(syntaxCount == 1 ? "" : "s")}";
			var message = $"Function requires {reqString} but was given {countString} ";

			return new Diagnostic(location.Line, location.Column, message, null, offendingText, null);
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

		internal static Diagnostic ReportInvalidParameterType(TextLocation location, TypeSymbol valueType, TypeSymbol requiredType, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, $"Parameter type {valueType} is not {requiredType}", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidFunctionDef(int line, int column, TextSpan? previousText, TextSpan offendingText, TextSpan? nextText) {
			return new Diagnostic(line, column, "Syntax error: Invalid function definition", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidFunctionDef(int line, int column, TextSpan offendingText) {
			return ReportInvalidFunctionDef(line, column, null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidType(TextLocation location, TextSpan previousText, TextSpan offendingText, TextSpan nextText, TypeSymbol requiredType) {
			return new Diagnostic(location.Line, location.Column, $"Invalid type. Required type: {requiredType}", previousText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidAssignmentTypeDef(TextLocation location, TextSpan previousText, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Variable types cannot be redefined", previousText, offendingText, null);
		}

		internal static Diagnostic ReportUndefinedVariable(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Variable is undefined", null, offendingText, null);
		}

		internal static Diagnostic ReportUnknownDeclKeyword(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Unknown variable declaration keyeword", null, offendingText, null);
		}

		internal static Diagnostic ReportUnknownTypeKeyword(TextLocation location, TextSpan prevText, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Unknown variable type", prevText, offendingText, null);
		}

		internal static Diagnostic ReportCannotCast(TextLocation location, TextSpan prevText, TextSpan offendingText, TypeSymbol fromType, TypeSymbol toType) {
			return new Diagnostic(location.Line, location.Column, $"Cannot implicitly convert {fromType} to {toType}", prevText, offendingText, null);
		}

		internal static Diagnostic ReportVoidType(TextLocation location, TextSpan? prevText, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Invalid use of void type", prevText, offendingText, null);
		}

		internal static Diagnostic ReportVoidType(TextLocation location, TextSpan offendingText) {
			return ReportVoidType(location, null, offendingText);
		}

		internal static Diagnostic ReportInvalidTypeDescription(int line, int column, TextSpan span) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportCannotRedefine(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, "Cannot redefine vadriable", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidOperator(TextLocation location, TextSpan prevText, TextSpan offendingText, TextSpan nextText, TypeSymbol valueType1, TypeSymbol valueType2) {
			return new Diagnostic(location.Line, location.Column, $"Operator is invalid for types {valueType1} and {valueType2}", prevText, offendingText, nextText);
		}

		internal static Diagnostic ReportInvalidOperator(TextLocation location, TextSpan prevText, TextSpan offendingText, TypeSymbol valueType) {
			return new Diagnostic(location.Line, location.Column, $"Operator is invalid for type {valueType}", prevText, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnType(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, $"Invalid return type", null, offendingText, null);
		}

		internal static Diagnostic ReportCannotRedefineFunction(int line, int column, TextSpan offendingText) {
			return new Diagnostic(line, column, $"Cannot redefine function", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnStatement(int line, int column, TextSpan span) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportInvalidBreakOrContinueStatement(TextLocation location, TextSpan offendingText, string keyword) {
			return new Diagnostic(location.Line, location.Column, $"Keyword \"{keyword}\" can only be used inside of a loop.", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnStatement(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, $"Retrun statements can only exist in a function", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnExpression(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, $"Void functions cannot return a value", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidReturnExpressionType(TextLocation location, TextSpan offendingText, TypeSymbol valueType, TypeSymbol returnType) {
			return new Diagnostic(location.Line, location.Column, $"Function returns {returnType}, but {valueType} was returned instead", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidExpression(int line, int column, TextSpan textSpan) {
			throw new NotImplementedException();
		}

		internal static Diagnostic ReportNoReturn(TextLocation location, TextSpan offendingText) {
			return new Diagnostic(location.Line, location.Column, $"Non void functions must return a value", null, offendingText, null);
		}

		internal static Diagnostic ReportInvalidTuple(int line, int column, TextSpan span) {
			throw new NotImplementedException();
		}
	}
}
