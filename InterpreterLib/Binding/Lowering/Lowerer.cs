using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Binding.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib.Binding.Lowering {
	internal class Lowerer : BoundTreeRewriter {

		private int counter;

		public Lowerer() {
			counter = 0;
		}

		private LabelSymbol CreateNextLabel() {
			return new LabelSymbol($"Label {counter++}");
		}

		public static BoundBlock Lower(BoundStatement statement) {
			Lowerer lowerer = new Lowerer();

			var lowered = lowerer.RewriteStatement(statement);
			return Flatten(lowered);
		}

		private static BoundBlock Flatten(BoundStatement statement) {
			var stack = new Stack<BoundStatement>();
			var statements = new List<BoundStatement>();

			stack.Push(statement);

			while(stack.Count > 0) {
				var current = stack.Pop();

				if (current is BoundBlock)
					foreach (var subStatement in ((BoundBlock)current).Statements.Reverse())
						stack.Push(subStatement);
				else
					statements.Add(current);
			}

			return new BoundBlock(statements);
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

			var condBranch = new BoundConditionalBranchStatement(falseLabel, statement.Condition, false);
			var endBranch = new BoundBranchStatement(endLabel);

			var statements = new List<BoundStatement>();

			statements.AddRange(new BoundStatement[]{
				condBranch,
				statement.TrueBranch,
				endBranch,
				new BoundLabel(falseLabel),
			});

			if (statement.FalseBranch != null)
				statements.Add(statement.FalseBranch);

			statements.Add(new BoundLabel(endLabel));

			return RewriteStatement(new BoundBlock(statements));
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

			var result = new BoundBlock(new BoundStatement[] {
				firstBranch,
				new BoundLabel(startLabel),
				statement.Body,
				new BoundLabel(conditionCheckLabel),
				condBranch
			});

			return RewriteStatement(result);
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

			List<BoundStatement> whileStatements = new List<BoundStatement>();

			if (statement.Body is BoundBlock)
				whileStatements.AddRange(((BoundBlock)statement.Body).Statements);
			else
				whileStatements.Add(statement.Body );

			whileStatements.Add(new BoundExpressionStatement(statement.Step));

			var result = new BoundBlock(new BoundStatement[] {
				statement.Assignment,
				new BoundWhileStatement(statement.Condition, new BoundBlock(whileStatements))
			});

			return RewriteStatement(result);
		}
	}
}
