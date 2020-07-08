using Antlr4.Runtime;
using InterpreterLib.Syntax.Tree.Global;
using System;
using System.Collections.Generic;
using System.Text;

namespace InterpreterLib.Syntax.Tree {
	public class SyntaxTree {

		public DiagnosticContainer Diagnostics { get; private set; }
		public IList<IToken> Tokens { get; private set; }
		public IVocabulary Vocabulary { get; private set; }
		internal CompilationUnitSyntax Root { get; private set; }

		public SyntaxTree(string input, bool parseFull = true) {
			var tokenStream = GetTokens(input);
			tokenStream.Fill();
			Tokens = tokenStream.GetTokens();

			if (parseFull) {
				CommonTokenStream tokens = GetTokens(input);
				GLangParser parser = new GLangParser(tokens);
				parser.RemoveErrorListeners();

				var ast = ASTProducer.CreateAST(parser.compilationUnit());

				if (ast.Value is CompilationUnitSyntax compSyntax)
					Root = compSyntax;
				else
					Root = null;

				Diagnostics.AddDiagnostics(ast.Diagnostics);
			}
		}

		private CommonTokenStream GetTokens(string input) {
			AntlrInputStream stream = new AntlrInputStream(input);

			GLangLexer lexer = new GLangLexer(stream);
			lexer.RemoveErrorListeners();

			Vocabulary = lexer.Vocabulary;
			return new CommonTokenStream(lexer);
		}
	}
}
