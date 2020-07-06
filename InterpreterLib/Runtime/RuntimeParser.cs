using Antlr4.Runtime;
using InterpreterLib.Syntax;
using InterpreterLib.Syntax.Tree;
using InterpreterLib.Syntax.Tree.Global;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Runtime {
	public class RuntimeParser {

		private DiagnosticResult<SyntaxNode> result;

		public IEnumerable<Diagnostic> Diagnostics => result.Diagnostics;
		internal CompilationUnitSyntax Node {
			get {
				if (!(result.Value is CompilationUnitSyntax compilationUnit))
					return null;

				return compilationUnit;
			}
		}

		public RuntimeParser(string input) {
			AntlrInputStream stream = new AntlrInputStream(input);

			GLangLexer lexer = new GLangLexer(stream);
			lexer.RemoveErrorListeners();

			CommonTokenStream tokens = new CommonTokenStream(lexer);
			GLangParser parser = new GLangParser(tokens);
			parser.RemoveErrorListeners();

			result = ASTProducer.CreateAST(parser.compilationUnit());
		}
	}
}
