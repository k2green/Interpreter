using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Binding {
	internal sealed class Binder : GLangBaseVisitor<BoundNode> {

		private BoundScope scope;
		private DiagnosticContainer diagnostics;

		public Binder(BoundScope parent) {
			scope = new BoundScope(parent);
		}

		public static DiagnosticResult<BoundGlobalScope> BindGlobalScope(BoundGlobalScope previous, IParseTree tree) {
			Binder binder = new Binder(CreateParentScopes(previous));

			BoundNode root = binder.Visit(tree);

			BoundGlobalScope glob = new BoundGlobalScope(previous, binder.diagnostics, binder.scope.GetVariables(), root);
			return new DiagnosticResult<BoundGlobalScope>(binder.diagnostics, glob);
		}

		private static BoundScope CreateParentScopes(BoundGlobalScope previous) {
			if (previous == null)
				return null;

			var stack = new Stack<BoundGlobalScope>();
			while (previous != null) {
				stack.Push(previous);
				previous = previous.Previous;
			}

			BoundScope parent = null;

			while (stack.Count > 0) {
				var scope = new BoundScope(parent);
				previous = stack.Pop();

				foreach (VariableSymbol variable in previous.Variables)
					scope.TryDefine(variable);

				parent = scope;
			}

			return parent;
		}

		private BoundError Error(Diagnostic diagnostic) {
			diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic, true, null);
		}

		private bool OnlyOne(params bool[] conditions) {
			return conditions.Count(b => b) == 1;
		}

		public override BoundNode VisitLiteral([NotNull] GLangParser.LiteralContext context) {
			bool hasIdentifier = context.IDENTIFIER() != null;
			bool hasInteger = context.INTEGER() != null;
			bool hasBool = context.BOOLEAN() != null;

			if (!OnlyOne(hasIdentifier, hasInteger, hasBool))
				return Error(Diagnostic.ReportInvalidLiteral(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasIdentifier && scope.TryLookup(context.IDENTIFIER().GetText(), out var variableSymbol))
				return new BoundVariableExpression(variableSymbol);

			if (hasInteger)
				return new BoundLiteral(int.Parse(context.INTEGER().GetText()));

			if (hasBool)
				return new BoundLiteral(bool.Parse(context.BOOLEAN().GetText()));

			throw new Exception($"OnlyOne failed to catch error. hasIdentifier = {hasIdentifier}, hasInteger = {hasIdentifier}, hasBool = {hasIdentifier}");
		}

		public override BoundNode VisitUnaryExpression([NotNull] GLangParser.UnaryExpressionContext context) {
			bool hasAtom = context.atom != null;
			bool hasBinExpr = context.L_PARENTHESIS() != null && context.binaryExpression() != null && context.R_PARENTHESIS() == null;
			bool hasSubUnExpr = context.op != null && context.unaryExpression() != null;

			if (!OnlyOne(hasAtom, hasBinExpr, hasSubUnExpr))
				return Error(Diagnostic.ReportInvalidUnaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasAtom)
				return Visit(context.atom);

			if (hasBinExpr)
				return Visit(context.binaryExpression());

			if (hasSubUnExpr) {

			}

			throw new Exception($"OnlyOne failed to catch error. hasAtom = {hasAtom}, hasBinExpr = {hasBinExpr}, hasSubUnExpr = {hasSubUnExpr}");
		}
	}
}
