using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace InterpreterLib.Output {
	public class SyntaxTreeOutput : OutputBase<SyntaxTree> {

		public SyntaxTreeOutput(SyntaxTree tree) : base(tree) {
			if (tree.Root != null) {
				Output(tree.Root, string.Empty);
			}
		}

		public void Output(Action<string, Color> write, Action newlineAction = null) => Document.Output((fragment) => write(fragment.Text, fragment.TextColour), newlineAction);

		private void Output(SyntaxNode node, string prefix) {
			switch (node.Type) {
				case SyntaxType.Literal:
					OutputLiteral((LiteralSyntax)node, prefix);
					break;
				case SyntaxType.UnaryExpression:
					OutputUnaryExpression((UnaryExpressionSyntax)node, prefix);
					break;
				case SyntaxType.BinaryExpression:
					OutputBinaryExpression((BinaryExpressionSyntax)node, prefix);
					break;
				case SyntaxType.VariableDeclaration:
					OutputVariableDeclaration((VariableDeclarationSyntax)node, prefix);
					break;
				case SyntaxType.Variable:
					OutputVariable((VariableSyntax)node, prefix);
					break;
				case SyntaxType.Assignment:
					OutputAssignmentExpression((AssignmentExpressionSyntax)node, prefix);
					break;
				case SyntaxType.Expression:
					OutputExpressionStatement((ExpressionStatementSyntax)node, prefix);
					break;
				case SyntaxType.IfStatement:
					OutputIfStatement((IfStatementSyntax)node, prefix);
					break;
				case SyntaxType.WhileLoop:
					OutputWhileLoop((WhileLoopSyntax)node, prefix);
					break;
				case SyntaxType.ForLoop:
					OutputForLoop((ForLoopSyntax)node, prefix);
					break;
				case SyntaxType.Block:
					OutputBlock((BlockSyntax)node, prefix);
					break;
				case SyntaxType.FunctionCall:
					OutputFunctionCall((FunctionCallSyntax)node, prefix);
					break;
				case SyntaxType.FunctionDeclaration:
					OutputFunctionDeclaration((FunctionDeclarationSyntax)node, prefix);
					break;
				case SyntaxType.TypedIdentifier:
					OutputTypedIdentifier((TypedIdentifierSyntax)node, prefix);
					break;
				case SyntaxType.CompilationUnit:
					OutputCompilationUnit((CompilationUnitSyntax)node, prefix);
					break;
				case SyntaxType.GlobalStatement:
					OutputGlobalStatement((GlobalStatementSyntax)node, prefix);
					break;
				case SyntaxType.Continue:
					OutputContinue((ContinueSyntax)node, prefix);
					break;
				case SyntaxType.Break:
					OutputBreak((BreakSyntax)node, prefix);
					break;
				case SyntaxType.Return:
					OutputReturn((ReturnSyntax)node, prefix);
					break;
				case SyntaxType.Tuple:
					OutputTuple((TupleSyntax)node, prefix);
					break;
				case SyntaxType.VariableIndexer:
					OutputVariableIndexer((VariableIndexerSyntax)node, prefix);
					break;
				case SyntaxType.Accessor:
					OutputAccessor((AccessorSyntax)node, prefix);
					break;
				case SyntaxType.Array:
					OutputArray((ArraySyntax)node, prefix);
					break;
				default: throw new NotImplementedException();
			}
		}

		private void OutputArray(ArraySyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("Array ", StatementColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			for (int index = 0; index < node.Values.Count; index++) {
				Output(node.Values[index], string.Empty);

				if(index < node.Values.Count - 1)
					builder.AddFragment(new OutputFragment(",", DefaultColour));
			}

			builder.AddFragment(new OutputFragment(")", DefaultColour));
		}

		private void OutputAccessor(AccessorSyntax node, string prefix) {
			Output(node.Item, prefix);

			if (!node.IsLast) {
				builder.AddFragment(new OutputFragment(".", DefaultColour));
				Output(node.Rest, string.Empty);
			}
		}

		private void OutputVariableIndexer(VariableIndexerSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			Output(node.Item, string.Empty);
			builder.AddFragment(new OutputFragment("[", DefaultColour));
			Output(node.Expression, string.Empty);
			builder.AddFragment(new OutputFragment("]", DefaultColour));
		}

		private void OutputTuple(TupleSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment($"{prefix}(", DefaultColour));

			foreach (var item in node.Items) {
				bool isLast = item == node.Items.Last();
				Output(item, string.Empty);

				if (!isLast)
					builder.AddFragment(new OutputFragment(", ", DefaultColour));
			}
			builder.AddFragment(new OutputFragment(")", DefaultColour));
		}

		private void OutputReturn(ReturnSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("return ", StatementColour));

			if (node.Expression != null)
				Output(node.Expression, string.Empty);
		}

		private void OutputCompilationUnit(CompilationUnitSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("Compilation Unit", StatementColour));
			builder.NewLine();

			foreach (var statement in node.Statements) {
				Output(statement, prefix + IndentString);
				builder.NewLine();
			}
		}

		private void OutputGlobalStatement(GlobalStatementSyntax node, string prefix) {
			Output(node.Statement, prefix);
		}

		private void OutputBreak(BreakSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("break", StatementColour));
		}

		private void OutputContinue(ContinueSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("continue", StatementColour));
		}

		private void OutputTypedIdentifier(TypedIdentifierSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.IdentifierName.ToString(), VariableColour));
			builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Definition.TypeDescription.ToString(), TypeColour));
		}

		private void OutputFunctionDeclaration(FunctionDeclarationSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment($"{node.KeywToken} ", StatementColour));
			builder.AddFragment(new OutputFragment(node.Identifier.ToString(), FunctionNameColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			var paramCount = node.Parameters.Count;
			for (int index = 0; index < paramCount; index++) {
				var parameter = node.Parameters[index];

				Output(parameter, string.Empty);

				if (index < paramCount - 1)
					builder.AddFragment(new OutputFragment(", ", DefaultColour));
			}

			builder.AddFragment(new OutputFragment(") {", DefaultColour));
			builder.NewLine();

			Output(node.Body, prefix + IndentString);

			builder.NewLine();
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("}", DefaultColour));
		}

		private void OutputFunctionCall(FunctionCallSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Identifier.ToString(), FunctionNameColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			var paramCount = node.Parameters.Count;
			for (int index = 0; index < paramCount; index++) {
				Output(node.Parameters[index], string.Empty);

				if (index < paramCount - 1)
					builder.AddFragment(new OutputFragment(", ", DefaultColour));
			}

			builder.AddFragment(new OutputFragment(")", DefaultColour));
		}

		private void OutputBlock(BlockSyntax node, string prefix) {
			foreach (var statement in node.Statements) {
				Output(statement, prefix + IndentString);
				builder.NewLine();
			}
		}

		private void OutputForLoop(ForLoopSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.ForToken.ToString(), StatementColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Assignment, string.Empty);
			builder.AddFragment(new OutputFragment(", ", StatementColour));
			Output(node.Condition, string.Empty);
			builder.AddFragment(new OutputFragment(", ", StatementColour));
			Output(node.Step, string.Empty);

			builder.AddFragment(new OutputFragment(") {", DefaultColour));
			builder.NewLine();

			Output(node.Body, prefix + IndentString);

			builder.NewLine();
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("}", DefaultColour));
		}

		private void OutputWhileLoop(WhileLoopSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.WhileToken.ToString(), StatementColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Condition, string.Empty);

			builder.AddFragment(new OutputFragment(") {", DefaultColour));
			builder.NewLine();

			Output(node.Body, prefix + IndentString);

			builder.NewLine();
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("}", DefaultColour));
		}

		private void OutputIfStatement(IfStatementSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.IfToken.ToString(), StatementColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Condition, string.Empty);

			builder.AddFragment(new OutputFragment(") {", DefaultColour));
			builder.NewLine();

			Output(node.TrueBranch, prefix + IndentString);
			builder.NewLine();
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("}", DefaultColour));

			if (node.FalseBranch != null) {
				builder.AddFragment(new OutputFragment(" else ", StatementColour));
				builder.AddFragment(new OutputFragment("{", DefaultColour));
				builder.NewLine();

				Output(node.FalseBranch, prefix + IndentString);
				builder.AddFragment(new OutputFragment(prefix, DefaultColour));
				builder.AddFragment(new OutputFragment("}", DefaultColour));
			}
		}

		private void OutputExpressionStatement(ExpressionStatementSyntax node, string prefix) {
			Output(node.Expression, prefix);
		}

		private void OutputAssignmentExpression(AssignmentExpressionSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment($"{node.IdentifierToken.ToString()} ", VariableColour));
			builder.AddFragment(new OutputFragment(" = ", StatementColour));

			Output(node.Expression, string.Empty);
		}

		private void OutputVariableDeclaration(VariableDeclarationSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment($"{node.KeywordToken} ", StatementColour));


			if (node.Identifier != null) {
				builder.AddFragment(new OutputFragment($"({node.Identifier.ToString()}", VariableColour));
				builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
				builder.AddFragment(new OutputFragment($"{node.Identifier.Definition.TypeDescription})", TypeColour));
			} else {
				builder.AddFragment(new OutputFragment(node.Initialiser.IdentifierToken.ToString(), VariableColour));
				builder.AddFragment(new OutputFragment(" = ", DefaultColour));

				Output(node.Initialiser.Expression, string.Empty) ;
			}
		}

		private void OutputBinaryExpression(BinaryExpressionSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			Output(node.LeftSyntax, string.Empty);
			builder.AddFragment(new OutputFragment($" {node.OpToken.ToString()} ", DefaultColour));
			Output(node.RightSyntax, string.Empty);
		}

		private void OutputUnaryExpression(UnaryExpressionSyntax node, string prefix) {
			bool expressionIsBinary = node.Expression is BinaryExpressionSyntax;

			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.OpToken.ToString(), DefaultColour));

			if (expressionIsBinary)
				builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Expression, string.Empty);

			if (expressionIsBinary)
				builder.AddFragment(new OutputFragment(")", DefaultColour));

		}

		private void OutputLiteral(LiteralSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.LiteralToken.ToString(), NumberColour));
		}

		private void OutputVariable(VariableSyntax node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.IdentifierToken.ToString(), VariableColour));
		}
	}
}
