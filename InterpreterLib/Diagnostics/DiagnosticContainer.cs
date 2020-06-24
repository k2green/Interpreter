using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace InterpreterLib {
	public sealed class DiagnosticContainer : IEnumerable<Diagnostic> {

		private List<Diagnostic> diagnostics;

		public DiagnosticContainer() {
			diagnostics = new List<Diagnostic>();
		}

		internal void AddDiagnostic(Diagnostic diagnostic) => diagnostics.Add(diagnostic);
		internal void AddDiagnostics(IEnumerable<Diagnostic> diagnosticEnum) => diagnostics.AddRange(diagnosticEnum);

		public IEnumerable<string> Messages => from x in diagnostics
											   select x.Message;

		public IEnumerator<Diagnostic> GetEnumerator() {
			return diagnostics.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}

		internal void ReportInvalidElse(int line, int column, string v) {
			throw new NotImplementedException();
		}
	}
}
