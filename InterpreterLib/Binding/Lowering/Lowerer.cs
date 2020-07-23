using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace InterpreterLib.Binding.Lowering {
	internal class Lowerer : BoundTreeRewriter {

		private int counter;

		public LabelSymbol EndLabel { get; }

		public Lowerer(LabelSymbol endLabel = null) {
			counter = 0;
			EndLabel = endLabel;
		}

		private LabelSymbol CreateNextLabel() {
			return new LabelSymbol($"Label {counter++}");
		}

		public static BoundBlock Lower(BoundStatement statement, LabelSymbol endLabel = null) {
			Lowerer lowerer = new Lowerer(endLabel);

			var lowered = lowerer.RewriteStatement(statement);
			return Flatten(lowered);
		}

		private static BoundBlock Flatten(BoundStatement statement) {
			var stack = new Stack<BoundStatement>();
			var statements = ImmutableArray.CreateBuilder<BoundStatement>();

			stack.Push(statement);

			while (stack.Count > 0) {
				var current = stack.Pop();

				if (current is BoundBlock block)
					foreach (var subStatement in block.Statements.Reverse())
						stack.Push(subStatement);
				else
					statements.Add(current);
			}

			return new BoundBlock(statements.ToImmutable());
		}

		protected override BoundStatement RewriteIfStatement(BoundIfStatement statement) {
			/*  if(<condition>) <true> else <false>
			 * 
			 *  condiditonal branch !<condition> false
			 * 	<true>
			 * 	branch end
			 * 	
			 * 	#false
			 * 	<false>
			 * 	#end
			 */

			var falseLabel = CreateNextLabel();
			var endLabel = CreateNextLabel();

			var endBranch = new BoundBranchStatement(endLabel);

			var statements = ImmutableArray.CreateBuilder<BoundStatement>();

			if (statement.FalseBranch != null) {
				var condBranch = new BoundConditionalBranchStatement(falseLabel, statement.Condition, false);
				statements.AddRange(condBranch, statement.TrueBranch, endBranch, new BoundLabel(falseLabel), statement.FalseBranch);
			} else {
				var condBranch = new BoundConditionalBranchStatement(endLabel, statement.Condition, false);
				statements.AddRange(condBranch, statement.TrueBranch);
			}

			statements.Add(new BoundLabel(endLabel));

			return RewriteStatement(new BoundBlock(statements.ToImmutable()));
		}

		protected override BoundStatement RewriteWhileStatement(BoundWhileStatement statement) {
			/*  while(<condition>) <body>
			 * 
			 *  branch conditionCheck
			 *  #start
			 *  <body>
			 *  #conditionCheck
			 *  conditional branch <condition> start
			 */

			var startLabel = CreateNextLabel();
			var conditionCheckLabel = CreateNextLabel();

			var firstBranch = new BoundBranchStatement(conditionCheckLabel);
			var condBranch = new BoundConditionalBranchStatement(startLabel, statement.Condition, true);

			var statements = ImmutableArray.CreateBuilder<BoundStatement>();
			statements.AddRange(new BoundStatement[] {
				firstBranch,
				new BoundLabel(startLabel),
				statement.Body
			});

			if (statement.AddContinue) {
				statements.Add(new BoundLabel(statement.ContinueLabel));
			}

			statements.AddRange(new BoundStatement[] {
				new BoundLabel(conditionCheckLabel),
				condBranch,
				new BoundLabel(statement.BreakLabel)
			});

			var rewrite = RewriteStatement(new BoundBlock(statements.ToImmutable()));

			return rewrite;
		}

		protected override BoundStatement RewriteForStatement(BoundForStatement statement) {
			/*  for(<assignment>, <condition>, <step>) <body>
			 * 
			 *  <assignment>
			 *  while (<condition>) {
			 *    body
			 *    step
			 *  }
			 */

			var whileStatements = ImmutableArray.CreateBuilder<BoundStatement>();

			whileStatements.AddRange(new BoundStatement[] {
				statement.Body,
				new BoundLabel(statement.ContinueLabel),
				new BoundExpressionStatement(statement.Step)
			});

			var whileStatement = new BoundWhileStatement(statement.Condition, new BoundBlock(whileStatements.ToImmutable()), statement.BreakLabel, statement.ContinueLabel, false);

			var finalStatements = ImmutableArray.CreateBuilder<BoundStatement>();

			finalStatements.AddRange(new BoundStatement[] {
				statement.Assignment,
				whileStatement
			});

			return RewriteStatement(new BoundBlock(finalStatements.ToImmutable()));
		}

		protected override BoundExpression RewriteBinaryExpression(BoundBinaryExpression expression) {
			var op = expression.Op;

			BoundExpression newLeft = RewriteExpression(expression.LeftExpression);
			BoundExpression newRight = RewriteExpression(expression.RightExpression);

			if (newLeft.ValueType != newRight.ValueType) {
				if (TypeConversionSymbol.TryFind(newLeft.ValueType, newRight.ValueType, out var leftConversion))
					newLeft = new BoundInternalTypeConversion(leftConversion, newLeft);
				else if (TypeConversionSymbol.TryFind(newRight.ValueType, newLeft.ValueType, out var rightConversion))
					newRight = new BoundInternalTypeConversion(rightConversion, newRight);
			}

			if (newLeft == expression.LeftExpression && newRight == expression.RightExpression)
				return expression;

			return new BoundBinaryExpression(newLeft, op, newRight);
		}

		protected override BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression assignment) {
			BoundExpression expression = RewriteExpression(assignment.Expression);

			if (expression.ValueType != assignment.Identifier.ValueType && TypeConversionSymbol.TryFind(expression.ValueType, assignment.Identifier.ValueType, out var symbol)) {
				expression = new BoundInternalTypeConversion(symbol, expression);
			}

			if (expression == assignment.Expression)
				return assignment;

			return new BoundAssignmentExpression(assignment.Identifier, expression);
		}

		protected override BoundStatement RewriteVariableDeclaration(BoundVariableDeclarationStatement statement) {
			if (statement.Initialiser == null)
				return statement;

			BoundExpression initialiser = RewriteExpression(statement.Initialiser);
			if (initialiser.ValueType != statement.Variable.ValueType && TypeConversionSymbol.TryFind(initialiser.ValueType, statement.Variable.ValueType, out var symbol)) {
				initialiser = new BoundInternalTypeConversion(symbol, initialiser);
			}

			if (initialiser == statement.Initialiser)
				return statement;

			return new BoundVariableDeclarationStatement(statement.Variable, initialiser);
		}

		protected override BoundExpression RewriteFunctionCall(BoundFunctionCall expression) {
			var newExpressions = ImmutableArray.CreateBuilder<BoundExpression>();
			bool isSame = true;

			for (int index = 0; index < expression.Parameters.Length; index++) {
				var parameter = expression.Parameters[index];
				var newParameter = RewriteExpression(parameter);

				if (!newParameter.ValueType.Equals(expression.ParameterTypes[index])
					&& TypeConversionSymbol.TryFind(newParameter.ValueType, expression.ParameterTypes[index], out var symbol)) {

					newParameter = new BoundInternalTypeConversion(symbol, parameter);
				}

				newExpressions.Add(newParameter);

				if (newParameter != parameter)
					isSame = false;
			}

			if (isSame)
				return expression;

			return new BoundFunctionCall(expression.Function, expression.PointerSymbol, newExpressions.ToImmutable());
		}

		protected override BoundStatement RewriteExpressionStatement(BoundExpressionStatement statement) {
			if (!statement.IsMarkedForRewrite || EndLabel == null)
				return base.RewriteExpressionStatement(statement);

			var newExpression = RewriteExpression(statement.Expression);

			return new BoundReturnStatement(newExpression, EndLabel);
		}

		protected override BoundExpression RewriteUnaryExpression(BoundUnaryExpression expression) {
			if (expression.Operand.Type != NodeType.UnaryExpression) {
				return base.RewriteUnaryExpression(expression);
			}

			var subUnary = (BoundUnaryExpression)expression.Operand;

			bool isIdentity =
				(expression.Op.OperatorType == UnaryOperatorType.Identity && subUnary.Op.OperatorType == UnaryOperatorType.Identity) ||
				(expression.Op.OperatorType == UnaryOperatorType.Negation && subUnary.Op.OperatorType == UnaryOperatorType.Negation) ||
				(expression.Op.OperatorType == UnaryOperatorType.LogicalNot && subUnary.Op.OperatorType == UnaryOperatorType.LogicalNot);

			bool isNegation =
				(expression.Op.OperatorType == UnaryOperatorType.Identity && subUnary.Op.OperatorType == UnaryOperatorType.Negation) ||
				(expression.Op.OperatorType == UnaryOperatorType.Negation && subUnary.Op.OperatorType == UnaryOperatorType.Identity);

			if (isIdentity)
				return RewriteExpression(subUnary.Operand);

			if (isNegation)
				return RewriteExpression(new BoundUnaryExpression(UnaryOperator.Bind("-", subUnary.Op.OperandType), subUnary.Operand));

			return base.RewriteUnaryExpression(expression);
		}
	}
}
