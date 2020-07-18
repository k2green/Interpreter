using InterpreterLib.Binding.Tree.Expressions;
using InterpreterLib.Symbols.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Evaluation.Objects {
	internal interface IndexableObject {
		object Index(object value);
	}
}
