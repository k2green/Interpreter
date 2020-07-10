using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Types;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Text;

namespace InterpreterLib.Output {
	public class ProgramOutput : OutputBase<BoundProgram> {

		internal ProgramOutput(BoundProgram program) : base(program) {
			foreach (var function in OutputItem.FunctionBodies.Keys) {
				OutputFunctionBody(function, OutputItem.FunctionBodies[function]);
				builder.NewLine();
			}

			Output(OutputItem.Statement, string.Empty);
		}

		private void Output(BoundNode node, string prefix) {
			switch (node.Type) {
				case NodeType.Literal:
					OutputLiteral((BoundLiteral)node, prefix);
					break;
				case NodeType.UnaryExpression:
					OutputUnaryExpression((BoundUnaryExpression)node, prefix);
					break;
				case NodeType.BinaryExpression:
					OutputBinaryExpression((BoundBinaryExpression)node, prefix);
					break;
				case NodeType.AssignmentExpression:
					OutputAssignmentExpression((BoundAssignmentExpression)node, prefix);
					break;
				case NodeType.Variable:
					OutputVariable((BoundVariableExpression)node, prefix);
					break;
				case NodeType.Block:
					OutputBlock((BoundBlock)node, prefix);
					break;
				case NodeType.VariableDeclaration:
					OutputVariableDeclaration((BoundVariableDeclarationStatement)node, prefix);
					break;
				case NodeType.Expression:
					OutputExpressionStatement((BoundExpressionStatement)node, prefix);
					break;
				case NodeType.Label:
					OutputLabel((BoundLabel)node, prefix);
					break;
				case NodeType.ConditionalBranch:
					OutputConditionalBranch((BoundConditionalBranchStatement)node, prefix);
					break;
				case NodeType.Branch:
					OutputBranch((BoundBranchStatement)node, prefix);
					break;
				case NodeType.TypeDefinition:
					OutputTypeDefinition((BoundTypeDefinition)node, prefix);
					break;
				case NodeType.FunctionCall:
					OutputFunctionCall((BoundFunctionCall)node, prefix);
					break;
				case NodeType.InternalTypeConversion:
					OutputInternalTypeConversion((BoundInternalTypeConversion)node, prefix);
					break;
			}
		}

		private void OutputFunctionBody(FunctionSymbol symbol, BoundBlock body) {
			builder.AddFragment(new OutputFragment("Function ", StatementColour));
			builder.AddFragment(new OutputFragment(symbol.Name, FunctionNameColour));
			builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
			builder.AddFragment(new OutputFragment(symbol.ReturnType.Name, TypeColour));
			builder.NewLine();

			var paramCount = symbol.Parameters.Length;
			if (paramCount > 0) {
				builder.AddFragment(new OutputFragment("Parameters ", StatementColour));

				for (int index = 0; index < paramCount - 1; index++) {
					builder.AddFragment(new OutputFragment(symbol.Parameters[index].Name, VariableColour));
					builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
					builder.AddFragment(new OutputFragment(symbol.Parameters[index].ValueType.Name, TypeColour));
					builder.AddFragment(new OutputFragment(", ", StatementColour));
				}

				builder.AddFragment(new OutputFragment(symbol.Parameters[paramCount - 1].Name, VariableColour));
				builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
				builder.AddFragment(new OutputFragment(symbol.Parameters[paramCount - 1].ValueType.Name, TypeColour));
				builder.NewLine();
			}

			OutputStatements(body.Statements, IndentString);
		}

		private void OutputInternalTypeConversion(BoundInternalTypeConversion node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.ValueType.Name, TypeColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Expression, string.Empty);

			builder.AddFragment(new OutputFragment(")", DefaultColour));
		}

		private void OutputTypeDefinition(BoundTypeDefinition node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.ValueType.Name, TypeColour));
		}

		private void OutputBranch(BoundBranchStatement node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("goto ", BranchColour));
			builder.AddFragment(new OutputFragment(node.Label.Name, LabelColour));
		}

		private void OutputConditionalBranch(BoundConditionalBranchStatement node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("goto ", BranchColour));
			builder.AddFragment(new OutputFragment(node.Label.Name, LabelColour));
			builder.AddFragment(new OutputFragment(node.BranchIfTrue ? " if " : " unless ", BranchColour));

			Output(node.Condition, string.Empty);
		}

		private void OutputLabel(BoundLabel node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("#" + node.Label.Name, LabelColour));
		}

		private void OutputExpressionStatement(BoundExpressionStatement node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			Output(node.Expression, string.Empty);
		}

		private void OutputVariableDeclaration(BoundVariableDeclarationStatement node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("declare (", StatementColour));
			builder.AddFragment(new OutputFragment(node.Variable.Name, VariableColour));
			builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Variable.ValueType.ToString(), TypeColour));
			builder.AddFragment(new OutputFragment(")", StatementColour));

			if (node.Initialiser != null) {
				builder.AddFragment(new OutputFragment(" = ", StatementColour));
				Output(node.Initialiser, string.Empty);
			}
		}

		private void OutputBlock(BoundBlock node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("Block", DefaultColour));
			builder.NewLine();

			OutputStatements(node.Statements, prefix + IndentString);
		}

		private void OutputStatements(IEnumerable<BoundStatement> statements, string prefix) {
			foreach (var statement in statements) {
				Output(statement, prefix);
				builder.NewLine();
			}
		}

		private void OutputAssignmentExpression(BoundAssignmentExpression node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment("assign (", StatementColour));
			builder.AddFragment(new OutputFragment(node.Identifier.Name, VariableColour));
			builder.AddFragment(new OutputFragment(DelimeterString, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Identifier.ValueType.Name, TypeColour));
			builder.AddFragment(new OutputFragment(") = ", StatementColour));

			Output(node.Expression, string.Empty);
		}

		private void OutputBinaryExpression(BoundBinaryExpression node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			Output(node.LeftExpression, string.Empty);
			builder.AddFragment(new OutputFragment($" {node.Op.TokenText} ", DefaultColour));
			Output(node.RightExpression, string.Empty);
		}

		private void OutputUnaryExpression(BoundUnaryExpression node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Op.TokenText, DefaultColour));
			bool isBinaryExpression = node.Operand is BoundBinaryExpression;

			if (isBinaryExpression)
				builder.AddFragment(new OutputFragment("(", DefaultColour));

			Output(node.Operand, string.Empty);

			if (isBinaryExpression)
				builder.AddFragment(new OutputFragment(")", DefaultColour));
		}

		private void OutputLiteral(BoundLiteral node, string prefix) {
			Color col = DefaultColour;
			string outString = node.Value.ToString();

			if (node.ValueType == TypeSymbol.String) {
				col = StringColour;
				outString = "\"" + outString + "\"";
			} else if (node.ValueType == TypeSymbol.Integer || node.ValueType == TypeSymbol.Double || node.ValueType == TypeSymbol.Byte) {
				col = NumberColour;
			} else if (node.ValueType == TypeSymbol.Boolean) {
				col = BooleanColour;
			}

			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(outString, col));
		}

		private void OutputVariable(BoundVariableExpression node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Variable.Name, VariableColour));
		}

		private void OutputFunctionCall(BoundFunctionCall node, string prefix) {
			builder.AddFragment(new OutputFragment(prefix, DefaultColour));
			builder.AddFragment(new OutputFragment(node.Function.Name, FunctionNameColour));
			builder.AddFragment(new OutputFragment("(", DefaultColour));

			var paramCount = node.Parameters.Length;
			if (paramCount > 0) {
				for (int index = 0; index < paramCount - 1; index++) {
					Output(node.Parameters[index], string.Empty);
					builder.AddFragment(new OutputFragment(", ", DefaultColour));
				}

				Output(node.Parameters[paramCount - 1], string.Empty);
			}

			builder.AddFragment(new OutputFragment(")", DefaultColour));
		}
	}
}
