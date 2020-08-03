using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Binding {
	internal sealed class FunctionBinder {

		public IEnumerable<FunctionSymbol> Functions => functions;

		private DiagnosticContainer diagnostics;
		private List<FunctionSymbol> functions;

		private BoundScope scope;

		public DiagnosticResult<BoundScope> BindFunctions(CompilationUnitSyntax syntax) {
			foreach (var statement in syntax.Statements) {
				Bind(statement);
			}

			return new DiagnosticResult<BoundScope>(diagnostics, scope);
		}

		public FunctionBinder(BoundScope programScope) {
			diagnostics = new DiagnosticContainer();
			functions = new List<FunctionSymbol>();
			scope = programScope;
		}

		private void Bind(SyntaxNode node) {
			if(node != null) {
				switch(node.Type) {
					case SyntaxType.Token:
						break;
					case SyntaxType.FunctionDeclaration:
						BindFunctionDeclaration((FunctionDeclarationSyntax)node);
						break;
					default:
						BindDefault(node);
						break;
				}
			}
		}

		private void BindDefault(SyntaxNode node) {
			foreach (var child in node.Children)
				Bind(child);
		}

		private void BindFunctionDeclaration(FunctionDeclarationSyntax syntax) {
			string funcName = syntax.Identifier == null ? syntax.ImplicitLabel : syntax.Identifier.ToString();
			var parameters = ImmutableArray.CreateBuilder<ParameterSymbol>();
			var returnType = TypeDescriptionBinder.BindTypeDescription(syntax.ReturnType.TypeDescription);

			foreach (var parameter in syntax.Parameters) {
				string parameterName = parameter.IdentifierName.ToString();
				var typeBind = TypeDescriptionBinder.BindTypeDescription(parameter.Definition.TypeDescription);

				int line = parameter.Definition.TypeDescription.Location.Line;
				int column = parameter.Definition.TypeDescription.Location.Column;
				var span = parameter.Definition.TypeDescription.Span;

				if (typeBind == null) {
					diagnostics.AddDiagnostic(Diagnostic.ReportInvalidParameterDefinition(line, column, span));
					return;
				}

				if (typeBind.Equals(ValueTypeSymbol.Void)) {
					diagnostics.AddDiagnostic(Diagnostic.ReportVoidType(parameter.Definition.TypeDescription.Location, span));
					return;
				}

				parameters.Add(new ParameterSymbol(parameterName, typeBind));
			}

			if (returnType == null) {
				diagnostics.AddDiagnostic(Diagnostic.ReportInvalidReturnType(syntax.ReturnType.Location, syntax.ReturnType.Span));
				return;
			}

			var functionSymbol = new FunctionSymbol(funcName, parameters.ToImmutable(), returnType, syntax);

			if (!scope.TryDefineFunction(functionSymbol)) {
				diagnostics.AddDiagnostic(Diagnostic.ReportCannotRedefineFunction(syntax.Identifier.Location, syntax.Identifier.Span));
			} else {
				functions.Add(functionSymbol);
			}
		}
	}
}
