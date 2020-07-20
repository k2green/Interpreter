using InterpreterLib.Binding;
using InterpreterLib.Binding.Tree.Statements;
using InterpreterLib.Diagnostics;
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
using InterpreterLib.Symbols.Binding;

namespace InterpreterLib.Runtime {
	public sealed class BindingEnvironment {
		private DiagnosticContainer diagnostics;
		private readonly bool chainDiagnostics;
		private BindingEnvironment previous;
		private CompilationUnitSyntax SyntaxRoot;

		public static string GraphDir {
			get {
				var appPath = Environment.GetCommandLineArgs()[0];
				var appDir = Path.GetDirectoryName(appPath);
				var fileDir = Path.Combine(appDir, "Control-flow-graphs");

				return fileDir;
			}
		}

		public static string MainGraphPath => Path.Combine(GraphDir, "Program.gv");
		public static string FunctionGraphDir => Path.Combine(GraphDir, "Functions");
		public static string FunctionGraphPath(FunctionSymbol function) => Path.Combine(FunctionGraphDir, $"{function.GetGraphFileName()}.gv");

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

			foreach(var function in GlobalScope.Functions) {
				Console.WriteLine($"{function.Name} => {function.ReturnType}");
			}

			OutputGraphs();

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

		private void OutputGraphs() {
			if (BoundProgram == null)
				return;

			var mainDirInfo = new DirectoryInfo(GraphDir);

			if (!mainDirInfo.Exists)
				mainDirInfo.Create();

			using (var streamWriter = new StreamWriter(MainGraphPath))
				BoundProgram.CreateMainGraph().WriteTo(streamWriter);

			var funcDirInfo = new DirectoryInfo(FunctionGraphDir);

			if (funcDirInfo.Exists)
				foreach (var subItem in funcDirInfo.GetFiles())
					subItem.Delete();
			else
				funcDirInfo.Create();

			var functionGraphs = BoundProgram.CreateFunctionGraphs();

			foreach (var funcSymbol in functionGraphs.Keys) {
				using (var streamWriter = new StreamWriter(FunctionGraphPath(funcSymbol)))
					functionGraphs[funcSymbol].WriteTo(streamWriter);
			}
		}
	}
}
