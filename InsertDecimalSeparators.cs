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
			string text = literal.Token.Text.Replace("_", "");
			string newText = text.WithDecimalSeparators();

			ExpressionSyntax newSyntax = 
				SyntaxFactory.ParseExpression(newText)
				.WithLeadingTrivia(literal.GetLeadingTrivia())
				.WithTrailingTrivia(literal.GetTrailingTrivia())
				.WithAdditionalAnnotations(Formatter.Annotation);

			SyntaxNode rootNode = await document.GetSyntaxRootAsync();
			SyntaxNode newRoot = rootNode.ReplaceNode(literal, newSyntax);

			return document.WithSyntaxRoot(newRoot);
		}
	}
}