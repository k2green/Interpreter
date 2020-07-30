using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Patterns;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Patterns;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace InterpreterLib.Binding {
	internal class PatternBinder {

		public DiagnosticContainer Diagnostics { get; }
		public BoundScope Scope { get; }

		public PatternBinder(BoundScope scope) {
			Scope = scope;
			Diagnostics = new DiagnosticContainer();
		}

		public BoundExpression BindAssignment(AssignmentExpressionSyntax assignmentExpr) {
			
		}

		public BoundVariableIdentifier BindIdentifier(PatternSyntax pattern, TypeSymbol type) {
			switch(pattern.Type) {
				case SyntaxType.VariablePattern:
					return BindVariableIdentifier((VariablePatternSyntax)pattern, type);
				case SyntaxType.TuplePattern:
					return BindTupleIdentifier((TuplePatternSyntax)pattern, type);
				default: throw new Exception($"Unexpected pattern type {pattern.Type}");
			}
		}

		private BoundVariableIdentifier BindVariableIdentifier(VariablePatternSyntax pattern, TypeSymbol type) {
			VariableSymbol variable;

			if(isDeclaration) {

			}
		}

		private BoundVariableIdentifier BindTupleIdentifier(TuplePatternSyntax pattern, TypeSymbol type {
			throw new NotImplementedException();
		}
	}
}
