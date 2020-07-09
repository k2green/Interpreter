using Antlr4.Runtime;
using InterpreterLib.Binding;
using InterpreterLib.Diagnostics;
using InterpreterLib.Runtime;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Global;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InterpreterLib.Syntax {
	public class SyntaxTreeWriter {
		private const string NEXT_CHILD = "  ├─";
		private const string NO_CHILD = "  │ ";
		private const string LAST_CHILD = "  └─";
		private const string SPACING = "    ";


		public TextWriter Writer { get; }

		public SyntaxTreeWriter(TextWriter writer) {
			Writer = writer;
		}

		public static void Write(SyntaxTree tree, TextWriter writer) {
			var treeWriter = new SyntaxTreeWriter(writer);

			treeWriter.Write(tree.Root, "└─", "  ");
		}

		private void WriteChildren(IEnumerable<SyntaxNode> children, string prefix) {
			SyntaxNode[] childArray = children.ToArray();

			int index;
			int max = childArray.Length - 1;

			while (childArray[max] == null && max > 0)
				max--;

			for (index = 0; index < max; index++) {
				var child = childArray[index];

				if (child != null)
					Write(child, prefix + NEXT_CHILD, prefix + NO_CHILD);
			}

			Write(childArray[max], prefix + LAST_CHILD, prefix + SPACING);
		}

		internal void Write(SyntaxNode node, string prefix1, string prefix2) {
			switch (node.Type) {
				case SyntaxType.Token:
					WriteToken((TokenSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Literal:
					WriteLiteral((LiteralSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.UnaryExpression:
					WriteUnaryExpression((UnaryExpressionSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.BinaryExpression:
					WriteBinaryExpression((BinaryExpressionSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Declaration:
					WriteVariableDeclaration((VariableDeclarationSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Variable:
					WriteVariable((VariableSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.TypeDefinition:
					WriteTypeDefinition((TypeDefinitionSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Assignment:
					WriteAssignmentExpression((AssignmentExpressionSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Error:
					WriteError((ErrorSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Expression:
					WriteExpressionStatement((ExpressionStatementSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.IfStatement:
					WriteIfStatement((IfStatementSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.WhileLoop:
					WriteWhileLoop((WhileLoopSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.ForLoop:
					WriteForLoop((ForLoopSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.Block:
					WriteBlock((BlockSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.FunctionCall:
					WriteFunctionCall((FunctionCallSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.TypedIdentifier:
					WriteTypedIdentifier((TypedIdentifierSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.ParameterDefinition:
					WriteParameterDefinition((ParameterDefinitionSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.FunctionDeclaration:
					WriteFunctionDeclaration((FunctionDeclarationSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.CompilationUnit:
					WriteCompilationUnit((CompilationUnitSyntax)node, prefix1, prefix2);
					break;
				case SyntaxType.GlobalStatement:
					WriteGlobalStatement((GlobalStatementSyntax)node, prefix1, prefix2);
					break;
			}
		}

		private void WriteGlobalStatement(GlobalStatementSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Global Statement:");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteCompilationUnit(CompilationUnitSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Compilation Unit:");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteFunctionDeclaration(FunctionDeclarationSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Function declaration");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteParameterDefinition(ParameterDefinitionSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Parameters");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteTypedIdentifier(TypedIdentifierSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Typed Identifier");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteFunctionCall(FunctionCallSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Function call");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteBlock(BlockSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Block");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteForLoop(ForLoopSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}For loop");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteWhileLoop(WhileLoopSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}While loop");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteIfStatement(IfStatementSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}If statement");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteExpressionStatement(ExpressionStatementSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Expression Statement");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteVariableDeclaration(VariableDeclarationSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Variable Declaration");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteTypeDefinition(TypeDefinitionSyntax node, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Type definition");

			WriteChildren(node.Children, prefix2);
		}

		private void WriteVariable(VariableSyntax variable, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Variable");

			WriteChildren(variable.Children, prefix2);
		}

		private void WriteAssignmentExpression(AssignmentExpressionSyntax expression, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Assignment expression:");

			WriteChildren(expression.Children, prefix2);
		}

		internal void WriteError(ErrorSyntax error, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}{error.Diagnostic}");
		}

		internal void WriteUnaryExpression(UnaryExpressionSyntax expression, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Unary Expression");

			WriteChildren(expression.Children, prefix2);
		}

		internal void WriteBinaryExpression(BinaryExpressionSyntax expression, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Binary Expression");

			WriteChildren(expression.Children, prefix2);
		}

		internal void WriteLiteral(LiteralSyntax literal, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Literal");

			WriteChildren(literal.Children, prefix2);
		}

		internal void WriteToken(TokenSyntax tokenSyntax, string prefix1, string prefix2) {
			Writer.WriteLine($"{prefix1}Token: {tokenSyntax.Token.Text}");
			Writer.WriteLine($"{prefix2}{LAST_CHILD}line={tokenSyntax.Token.Line} column={tokenSyntax.Token.Column}");
		}
	}
}
