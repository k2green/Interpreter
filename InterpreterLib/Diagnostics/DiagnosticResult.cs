using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib {
	public struct DiagnosticResult<T> {

		public IEnumerable<Diagnostic> Diagnostics { get; }
		public T Value { get; }

		public DiagnosticResult(IEnumerable<Diagnostic> diagnostics, T value) {
			Diagnostics = diagnostics;
			Value = value;
		}
	}
}
