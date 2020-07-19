using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Binding;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree.Statements {
	internal sealed class BoundReturnStatement : BoundStatement {
		public override NodeType Type => NodeType.Return;

		public BoundExpression ReturnExpression { get; }
		public LabelSymbol EndLabel { get; }

		public BoundReturnStatement(BoundExpression returnExpression, LabelSymbol endLabel) {
			ReturnExpression = returnExpression;
			EndLabel = endLabel;
		}

		public override string ToString() {
			var builder = new StringBuilder();

			builder.Append("return ");
			builder.Append(ReturnExpression);

			return builder.ToString();
		}
	}
}
