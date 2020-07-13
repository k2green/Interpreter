using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using InterpreterLib.Binding;
using InterpreterLib.Binding.Lowering;
using InterpreterLib.Binding.Tree;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Types;
using InterpreterLib.Diagnostics;
using InterpreterLib.Syntax;
using InterpreterLib.Syntax.Tree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InterpreterLib.Syntax.Tree.Global;
using System.Drawing;
using InterpreterLib.Output;
using System.IO;
using InterpreterLib.Binding.ControlFlow;

namespace InterpreterLib.Runtime {
	public sealed class BindingEnvironment {
		private DiagnosticContainer diagnostics;
		private readonly bool chainDiagnostics;
		private BindingEnvironment previous;
		private CompilationUnitSyntax SyntaxRoot;

		private BoundProgram boundProgram;
		internal BoundProgram BoundProgram {
			get {
				if (boundProgram == null) {
					if (GlobalScope == null || GlobalScope.Root == null || diagnostics.Any())
						return null;

					var program = Binder.BindProgram(GlobalScope);
					diagnostics.AddDiagnostics(program.Diagnostics);

					if (program.Value == null)
						return null;

					Interlocked.CompareExchange<BoundProgram>(ref boundProgram, program.Value, null);
					diagnostics.AddDiagnostics(program.Diagnostics);
				}

				return boundProgram;
			}
		}

		public IEnumerable<Diagnostic> Diagnostics => diagnostics;

		public static BindingEnvironment CreateEnvironment(SyntaxTree input, bool chainDiagnostics) {
			if (input.Diagnostics.Any())
				return null;

			return new BindingEnvironment(input.Root, chainDiagnostics, null);
		}

		public BindingEnvironment ContinueWith(SyntaxTree input) {
			return new BindingEnvironment(input.Root, chainDiagnostics, this);
		}

		private BoundGlobalScope globalScope;
		internal BoundGlobalScope GlobalScope {
			get {
				if (globalScope == null) {
					var prevScope = previous == null ? null : previous.GlobalScope;
					var binderResult = Binder.BindGlobalScope(prevScope, SyntaxRoot);
					Interlocked.CompareExchange<BoundGlobalScope>(ref globalScope, binderResult.Value, null);
					diagnostics.AddDiagnostics(binderResult.Diagnostics);
				}

				return globalScope;
			}
		}

		private BindingEnvironment(CompilationUnitSyntax input, bool chainDiagnostics, BindingEnvironment previous) {
			this.chainDiagnostics = chainDiagnostics;
			this.previous = previous;
			diagnostics = new DiagnosticContainer();

			if (previous != null && chainDiagnostics) {
				diagnostics.AddDiagnostics(previous.diagnostics);
			}

			SyntaxRoot = input;
		}

		public DiagnosticResult<object> Evaluate(Dictionary<VariableSymbol, object> variables) {
			if (BoundProgram == null)
				return new DiagnosticResult<object>(diagnostics, null);

			var block = !BoundProgram.Statement.Statements.Any() && BoundProgram.FunctionBodies.Any()
						? BoundProgram.FunctionBodies.Values.Last()
						: BoundProgram.Statement;

			OutputGraph("graph.gv", block);
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

		private void OutputGraph(string pathName, BoundBlock block) {
			var appPath = Environment.GetCommandLineArgs()[0];
			var appDir = Path.GetDirectoryName(appPath);
			var filePath = Path.Combine(appDir, pathName);

			var cfg = ControlFlowGraph.CreateGraph(block);

			Console.WriteLine(filePath);

			using (StreamWriter writer = new StreamWriter(filePath))
				cfg.WriteTo(writer);
		}
	}
}
