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

		private BoundError Error(Diagnostic diagnostic) => Error(diagnostic, true, null);

		private BoundError Error(Diagnostic diagnostic, bool isCausing, params BoundNode[] children) {
			if (diagnostic != null)
				diagnostics.AddDiagnostic(diagnostic);

			return new BoundError(diagnostic, isCausing, children);
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
				var operand = Visit(context.unaryExpression());

				if (!(operand is BoundExpression))
					return Error(null, false, operand);

				var op = UnaryOperator.Bind(context.op.Text, ((BoundExpression)operand).ValueType);

				if (op == null)
					return Error(Diagnostic.ReportInvalidUnaryOperator(context.Start.Line, context.Start.Column, context.op.Text, ((BoundExpression)operand).ValueType), true, operand);

				return new BoundUnaryExpression(op, (BoundExpression)operand);
			}

			throw new Exception($"OnlyOne failed to catch error. hasAtom = {hasAtom}, hasBinExpr = {hasBinExpr}, hasSubUnExpr = {hasSubUnExpr}");
		}

		public override BoundNode VisitBinaryExpression([NotNull] GLangParser.BinaryExpressionContext context) {
			bool hasAtom = context.atom != null;
			bool hasExpression = context.left != null && context.op != null && context.right != null;

			if (!OnlyOne(hasAtom, hasExpression))
				return Error(Diagnostic.ReportInvalidBinaryExpression(context.Start.Line, context.Start.Column, context.GetText()));

			if (hasAtom)
				return Visit(context.atom);

			if (hasExpression) {
				var left = Visit(context.left);
				var right = Visit(context.right);

				if (!(left is BoundExpression) || !(right is BoundExpression))
					return Error(null, false, left, right);

				var op = BinaryOperator.Bind(context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType);

				if (op == null)
					return Error(Diagnostic.ReportInvalidBinaryOperator(context.Start.Line, context.Start.Column, context.op.Text, ((BoundExpression)left).ValueType, ((BoundExpression)right).ValueType), true, left, right);

				return new BoundBinaryExpression((BoundExpression)left, op, (BoundExpression)right);
			}


			throw new Exception($"OnlyOne failed to catch error. hasAtom = {hasAtom}, hasExpression = {hasExpression}");
		}

		public override BoundNode VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context) {
			return VisitVariableDeclaration(context, null);
		}

		private BoundNode VisitVariableDeclaration([NotNull] GLangParser.VariableDeclarationContext context, TypeSymbol type) {
			bool hasVarDecl = context.DECL_VARIABLE() != null;
			bool hasIdentifier = context.IDENTIFIER() != null;
			bool hasDelimeter = context.TYPE_DELIMETER() != null;
			bool hasTypeDef = context.TYPE_NAME() != null;
			bool requireTypeCondition = type == null && !hasDelimeter && !hasTypeDef;
			bool isReadOnly;

			if (!hasVarDecl || !hasIdentifier || hasDelimeter ^ hasTypeDef || requireTypeCondition)
				return Error(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));

			if (type == null) {
				switch (context.TYPE_NAME().GetText()) {
					case "int": type = TypeSymbol.Integer; break;
					case "bool": type = TypeSymbol.Boolean; break;

					default:
						return Error(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));
				}
			}

			switch (context.DECL_VARIABLE().GetText()) { 
				case "var": isReadOnly = false; break;
				case "val": isReadOnly = true; break;

				default:
					return Error(Diagnostic.ReportInvalidDeclaration(context.Start.Line, context.Start.Column, context.GetText()));
			}

			var variable = new VariableSymbol(context.IDENTIFIER().GetText(), isReadOnly, type);

			if (!scope.TryDefine(variable))
				return Error(Diagnostic.ReportRedefineVariable(context.Start.Line, context.Start.Column, variable));

			return new BoundDeclarationStatement(new BoundVariableExpression(variable));
		}
	}
}
