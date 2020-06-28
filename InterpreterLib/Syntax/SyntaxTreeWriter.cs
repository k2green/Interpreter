using Antlr4.Runtime;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Expressions;
using InterpreterLib.Syntax.Tree.Statements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace InterpreterLib.Syntax {
	public class SyntaxTreeWriter {
		private const string NEXT_CHILD = "  ├─";
		private const string NO_CHILD   = "  │ ";
		private const string LAST_CHILD = "  └─";
		private const string SPACING    = "    ";


		public TextWriter Writer {get; }

		private SyntaxTreeWriter(TextWriter writer) {
			Writer = writer;
		}

		public static void ParseAndWriteTree(TextWriter writer, string input) {
			AntlrInputStream stream = new AntlrInputStream(input);

			GLangLexer lexer = new GLangLexer(stream);
			lexer.RemoveErrorListeners();

			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GLangParser parser = new GLangParser(tokens);
			parser.RemoveErrorListeners();

			ASTProducer astProd = new ASTProducer();
			SyntaxTreeWriter treeWriter = new SyntaxTreeWriter(writer);
			treeWriter.Write(astProd.Visit(parser.statement()), "  ", "  ");
		}

		private void WriteChildren(IEnumerable<SyntaxNode> children, string prefix) {
			SyntaxNode[] childArray = children.ToArray();

			int index;
			int max = childArray.Length - 1;

			while (childArray[max] == null && max > 0)
				max--;

			for (index = 0; index < max; index++) {
				var child = childArray[index];

				if(child != null)
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
			}
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
			Writer.WriteLine($"{prefix1}Assignment Expression");

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
			Writer.WriteLine($"{prefix1}Token: {tokenSyntax.Token.Text} line={tokenSyntax.Token.Line} column={tokenSyntax.Token.Column}");
		}
	}
}
