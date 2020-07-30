using InterpreterLib.Binding;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Global;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using InterpreterLib.Symbols.Binding;
using InterpreterLib.Diagnostics;
using InterpreterLib.Output;
using System.Drawing;

namespace InterpreterLib.Runtime {
	public class Interperatation {

		public SyntaxTree InputTree { get; }

		private DiagnosticContainer diagnostics;
		public IEnumerable<Diagnostic> Diagnostics => diagnostics;

		private BoundGlobalScope globalScope;
		internal BoundGlobalScope GlobalScope => globalScope;

		private BoundProgram boundProgram;
		internal BoundProgram BoundProgram => boundProgram;

		public Interperatation(SyntaxTree input, Interperatation previous = null) {
			InputTree = input;
			diagnostics = new DiagnosticContainer();

			CreateGlobalScope(input.Root, previous);
			CreateProgram();
		}

		public BoundProgram CreateProgram() {
			if (GlobalScope == null || GlobalScope.Root == null || diagnostics.Any())
				return null;

			var program = Binder.BindProgram(GlobalScope);
			diagnostics.AddDiagnostics(program.Diagnostics);

			if (program.Value == null)
				return null;

			Interlocked.CompareExchange(ref boundProgram, program.Value, null);
			diagnostics.AddDiagnostics(program.Diagnostics);

			return program.Value;
		}

		private BoundGlobalScope CreateGlobalScope(CompilationUnitSyntax compilationUnit, Interperatation previous) {
			var prevScope = previous == null ? null : previous.GlobalScope;
			var binderResult = Binder.BindGlobalScope(prevScope, compilationUnit);
			Interlocked.CompareExchange(ref globalScope, binderResult.Value, null);
			diagnostics.AddDiagnostics(binderResult.Diagnostics);

			return GlobalScope;
		}

		public DiagnosticResult<object> Evaluate(Dictionary<VariableSymbol, object> variables) {
			if (BoundProgram == null)
				return new DiagnosticResult<object>(diagnostics, null);

			foreach (var function in GlobalScope.Functions) {
				Console.WriteLine($"{function.Name} => {function.ReturnType}");
			}

			Evaluator evaluator = new Evaluator(boundProgram, variables);

			var evalResult = evaluator.Evaluate();
			diagnostics.AddDiagnostics(evalResult.Diagnostics);
			return new DiagnosticResult<object>(diagnostics, evalResult.Value);
		}

		public void PrintProgram(Action<string, Color> printAction, Action newlineAction) {
			if (BoundProgram == null)
				return;

			var output = new ProgramOutput(BoundProgram);
			output.Document.Output((frag) => printAction(frag.Text, frag.TextColour), newlineAction);
		}
	}
}
