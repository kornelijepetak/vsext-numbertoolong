using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System.Text;
using Microsoft.CodeAnalysis.Formatting;

namespace NumberTooLong
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InsertDecimalSeparators)), Shared]
	public class InsertDecimalSeparators : CodeFixProvider
	{
		private const string diagnosticId = "NumberTooLongAnalyzer";
		private const string title = "Insert decimal separators";

		public sealed override ImmutableArray<string> FixableDiagnosticIds
		{
			get { return ImmutableArray.Create(diagnosticId); }
		}

		public sealed override FixAllProvider GetFixAllProvider()
		{
			return WellKnownFixAllProviders.BatchFixer;
		}

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			Diagnostic diagnostic = context.Diagnostics.First();
			TextSpan span = diagnostic.Location.SourceSpan;

			SyntaxNode root = await context
				.Document
				.GetSyntaxRootAsync(context.CancellationToken)
				.ConfigureAwait(false);

			LiteralExpressionSyntax literalSyntax =
				root.FindToken(span.Start).Parent as LiteralExpressionSyntax;

			context.RegisterCodeFix(
				CodeAction.Create(
					title,
					c => InsertDecimalSeparatorsAsync(context.Document, literalSyntax, c),
					equivalenceKey: title),
				diagnostic);
		}

		private async Task<Document> InsertDecimalSeparatorsAsync(
			Document document,
			LiteralExpressionSyntax literal,
			CancellationToken cancellationToken)
		{
			string text = literal.Token.Text;
			string newText = insertDecimalSeparators(text);

			var newSyntax = SyntaxFactory.ParseExpression(newText)
				.WithLeadingTrivia(literal.GetLeadingTrivia())
				.WithTrailingTrivia(literal.GetTrailingTrivia())
				.WithAdditionalAnnotations(Formatter.Annotation);

			var root = await document.GetSyntaxRootAsync();
			var newRoot = root.ReplaceNode(literal, newSyntax);

			var newDocument = document.WithSyntaxRoot(newRoot);
			return newDocument;
		}

		private string insertDecimalSeparators(string number)
		{
			int insertBeforeModIndex = number.Length % 3;
			StringBuilder newNumberText = new StringBuilder();
			for (int i = 0; i < number.Length; i++)
			{
				if (i > 0 && i % 3 == insertBeforeModIndex)
					newNumberText.Append("_");

				newNumberText.Append(number[i]);
			}

			return newNumberText.ToString();
		}
	}
}