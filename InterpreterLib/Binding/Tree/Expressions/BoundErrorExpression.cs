using InterpreterLib.Symbols.Types;
using System;

namespace InterpreterLib.Binding.Tree.Expressions {
	internal sealed class BoundErrorExpression : BoundExpression {
		public override TypeSymbol ValueType => throw new Exception("Error values should not be accessed");

		public override NodeType Type => NodeType.Error;

		public Diagnostic Diagnostic { get; }

		public BoundErrorExpression(Diagnostic diagnostic) {
			Diagnostic = diagnostic;
		}

		public override string ToString() => "Error";
	}
}
