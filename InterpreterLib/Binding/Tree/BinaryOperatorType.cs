using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Binding.Tree {
	internal enum BinaryOperatorType {
		Addition, Subtraction, Multiplication, Division, Power, Modulus, Equality, LogicalAnd, LogicalOr,
		LogicalXOr,
		GreaterThan,
		LesserThan,
		StrictGreaterThan,
		StrinLesserThan
	}
}
