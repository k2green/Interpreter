using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InterpreterLib.Binding.ControlFlow {
	internal sealed class ControlFlowGraph {

		public ControlFlowGraph(BasicBlock start, BasicBlock end, List<BasicBlock> blocks, List<BasicBlockEdge> edges) {
			Start = start;
			End = end;
			Blocks = blocks;
			Edges = edges;
		}

		public static string ConvertString(string inputStr) => inputStr.Replace("\"", "\\\"").Replace(Environment.NewLine, "\\l");

		public void WriteTo(TextWriter writer) {
			writer.WriteLine("digraph ControlFlow {");

			var blockIds = new Dictionary<BasicBlock, string>();
			for (int id = 0; id < Blocks.Count; id++) {
				blockIds.Add(Blocks[id], $"N{id}");
			}

			foreach (var block in Blocks) {
				string id = blockIds[block];
				var label = block.ToString();
				writer.WriteLine($"    {id} [label = \"{label}\", shape = box]");
			}

			foreach (var edge in Edges) {
				string fromId = blockIds[edge.From];
				string toId = blockIds[edge.To];
				var label = edge.Condition == null ? "" : $"[label = \"{edge.Condition.ToString()}\"]";

				writer.WriteLine($"    {fromId} -> {toId} {label}");
			}

			writer.WriteLine("}");
		}

		public BasicBlock Start { get; }
		public BasicBlock End { get; }
		public List<BasicBlock> Blocks { get; }
		public List<BasicBlockEdge> Edges { get; }

		public static ControlFlowGraph CreateGraph(BoundBlock block) {
			var blockBuilder = new BasicBlockBuilder();
			var edgeBuilder = new GraphBuilder();
			var blocks = blockBuilder.Build(block);

			return edgeBuilder.Build(blocks);
		}

		internal sealed class GraphBuilder {
			private Dictionary<BoundStatement, BasicBlock> blockFromStatement;
			private Dictionary<LabelSymbol, BasicBlock> blockFromLabel;
			private List<BasicBlockEdge> edges;

			private BasicBlock start;
			private BasicBlock end;

			public GraphBuilder() {
				blockFromStatement = new Dictionary<BoundStatement, BasicBlock>();
				blockFromLabel = new Dictionary<LabelSymbol, BasicBlock>();
				edges = new List<BasicBlockEdge>();

				start = new BasicBlock(true);
				end = new BasicBlock(false);
			}

			public ControlFlowGraph Build(List<BasicBlock> blocks) {
				SetupFields(blocks);

				if (!blocks.Any())
					Connect(start, end);
				else
					Connect(start, blocks.First());


				for (int index = 0; index < blocks.Count; index++) {
					var currentBlock = blocks[index];
					var nextBlock = index == blocks.Count - 1 ? end : blocks[index + 1];

					foreach (var statement in currentBlock.Statements) {
						var isLast = statement == currentBlock.Statements.Last();

						switch (statement.Type) {
							case NodeType.Branch:
								var brStat = (BoundBranchStatement)statement;
								var branchTo = blockFromLabel[brStat.Label];
								Connect(currentBlock, branchTo);
								break;
							case NodeType.ConditionalBranch:
								var condBrStat = (BoundConditionalBranchStatement)statement;
								var trueBranch = blockFromLabel[condBrStat.Label];
								var negate = Negate(condBrStat.Condition);

								var trueCondition = condBrStat.BranchIfTrue ? condBrStat.Condition : negate;
								var falseCondition = condBrStat.BranchIfTrue ? negate : condBrStat.Condition;

								Connect(currentBlock, trueBranch, trueCondition);
								Connect(currentBlock, nextBlock, falseCondition);
								break;
							case NodeType.Label:
								if (isLast)
									Connect(currentBlock, nextBlock);
								break;
							case NodeType.VariableDeclaration:
							case NodeType.Expression:
								if (isLast)
									Connect(currentBlock, nextBlock);
								break;
							default: throw new Exception("Unhandled case");
						}
					}
				}

				blocks.Insert(0, start);
				blocks.Add(end);

				return new ControlFlowGraph(start, end, blocks, edges);
			}

			private BoundExpression Negate(BoundExpression condition) {
				if (condition is BoundLiteral literal) {
					var value = (bool)literal.Value;
					return new BoundLiteral(!value);
				}

				var op = UnaryOperator.Bind("!", ValueTypeSymbol.Boolean);
				return new BoundUnaryExpression(op, condition);
			}

			private void Connect(BasicBlock from, BasicBlock to, BoundExpression condition = null) {
				var edge = new BasicBlockEdge(from, to, condition);

				edges.Add(edge);
				from.OutgoinfEdges.Add(edge);
				to.IncomingEdges.Add(edge);
			}

			private void SetupFields(List<BasicBlock> blocks) {
				foreach (var block in blocks) {
					foreach (var statement in block.Statements) {
						blockFromStatement.Add(statement, block);

						if (statement is BoundLabel label)
							blockFromLabel.Add(label.Label, block);
					}
				}
			}
		}

		internal sealed class BasicBlockBuilder {

			private List<BoundStatement> currentStatements;
			private List<BasicBlock> blocks;

			public BasicBlockBuilder() {
				currentStatements = new List<BoundStatement>();
				blocks = new List<BasicBlock>();
			}

			public List<BasicBlock> Build(BoundBlock block) {
				foreach (var statement in block.Statements) {
					switch (statement.Type) {
						case NodeType.Label:
							StartBlock();
							currentStatements.Add(statement);
							break;
						case NodeType.ConditionalBranch:
						case NodeType.Branch:
							currentStatements.Add(statement);
							StartBlock();
							break;
						case NodeType.VariableDeclaration:
						case NodeType.Expression:
							currentStatements.Add(statement);
							break;
						default: throw new Exception("Unhandled case");
					}
				}

				EndBlock();

				return blocks;
			}

			public void StartBlock() {
				EndBlock();
			}

			public void EndBlock() {
				if (currentStatements.Any()) {
					var block = new BasicBlock();
					block.Statements.AddRange(currentStatements);
					currentStatements.Clear();
					blocks.Add(block);
				}
			}
		}

		internal sealed class BasicBlock {

			public BasicBlock() { }

			public BasicBlock(bool isStart) {
				IsStart = isStart;
				IsEnd = !isStart;
			}

			public bool IsStart { get; }
			public bool IsEnd { get; }
			public List<BoundStatement> Statements { get; } = new List<BoundStatement>();
			public List<BasicBlockEdge> IncomingEdges { get; } = new List<BasicBlockEdge>();
			public List<BasicBlockEdge> OutgoinfEdges { get; } = new List<BasicBlockEdge>();

			public override string ToString() {
				if (IsStart) return "<start>";
				if (IsEnd) return "<end>";

				var builder = new StringBuilder();
				var statCount = Statements.Count;

				for (int index = 0; index < statCount; index++) {
					builder.Append(Statements[index].ToString());

					if (index < statCount - 1)
						builder.Append(Environment.NewLine);
				}

				return ConvertString(builder.ToString());
			}
		}

		internal sealed class BasicBlockEdge {

			public BasicBlockEdge(BasicBlock from, BasicBlock to, BoundExpression condition) {
				From = from;
				To = to;
				Condition = condition;
			}

			public BasicBlock From { get; }
			public BasicBlock To { get; }
			public BoundExpression Condition { get; }
		}
	}
}
