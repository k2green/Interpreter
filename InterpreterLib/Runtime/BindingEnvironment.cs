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

		public IEnumerable<Diagnostic> Diagnostics => diagnostics;

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

		private List<Interperatation> interperatations;
		public Interperatation CurrentInterpretation {
			get {
				if (interperatations.Count == 0 || interperatations.Count - 1 < 0)
					return null;

				return interperatations[interperatations.Count - 1];
			}
		}

		public bool ContinueWith(SyntaxTree input) {
			if (input == null || input.Diagnostics.Any()) {
				if (chainDiagnostics)
					diagnostics.AddDiagnostics(input.Diagnostics);

				return false;
			}

			if (interperatations == null)
				interperatations = new List<Interperatation>();

			var interperatation = new Interperatation(input, CurrentInterpretation);

			if(interperatation.Diagnostics.Any()) {
				if (chainDiagnostics)
					diagnostics.AddDiagnostics(interperatation.Diagnostics);

				return false;
			} else {
				interperatations.Add(interperatation);
				return true;
			}
		}

		public BindingEnvironment(bool chainDiagnostics) {
			this.chainDiagnostics = chainDiagnostics;
			diagnostics = new DiagnosticContainer();
		}

		private void OutputGraphs() {
			if (CurrentInterpretation.BoundProgram == null)
				return;

			var mainDirInfo = new DirectoryInfo(GraphDir);

			if (!mainDirInfo.Exists)
				mainDirInfo.Create();

			using (var streamWriter = new StreamWriter(MainGraphPath))
				CurrentInterpretation.BoundProgram.CreateMainGraph().WriteTo(streamWriter);

			var funcDirInfo = new DirectoryInfo(FunctionGraphDir);

			if (funcDirInfo.Exists)
				foreach (var subItem in funcDirInfo.GetFiles())
					subItem.Delete();
			else
				funcDirInfo.Create();

			var functionGraphs = CurrentInterpretation.BoundProgram.CreateFunctionGraphs();

			foreach (var funcSymbol in functionGraphs.Keys) {
				using (var streamWriter = new StreamWriter(FunctionGraphPath(funcSymbol)))
					functionGraphs[funcSymbol].WriteTo(streamWriter);
			}
		}
	}
}
