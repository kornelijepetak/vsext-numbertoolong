using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NumberTooLong
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class NumberTooLongAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "NumberTooLongAnalyzer";

		private const string message = "The number may be hard to read. How about some digit separators?";

		internal static DiagnosticDescriptor Rule
			= new DiagnosticDescriptor(
				id: DiagnosticId,
				title: DiagnosticId,
				messageFormat: "NumberTooLongAnalyzer '{0}'",
				category: "Hints",
				defaultSeverity: DiagnosticSeverity.Warning,
				isEnabledByDefault: true);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
			=> ImmutableArray.Create(Rule);

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterSyntaxNodeAction(handle, SyntaxKind.NumericLiteralExpression);
		}

		private void handle(SyntaxNodeAnalysisContext context)
		{
			LiteralExpressionSyntax numberSyntax = (LiteralExpressionSyntax)context.Node;

			if (!numberSyntax.IsKind(SyntaxKind.NumericLiteralExpression))
				return;

			string numberText = numberSyntax.Token.Text;

			Type constantType = context.SemanticModel
				.GetConstantValue(numberSyntax)
				.Value
				.GetType();

			if (constantType != typeof(int) && constantType != typeof(long))
				return;

			string rawNumberText = numberText.Replace("_", "");

			if (rawNumberText.Length < 6)
				return;

			int underscoreCount = numberText.GetUnderscoreCount();
			if (underscoreCount >= (rawNumberText.Length - 1) / 3)
				return;

			context.ReportDiagnostic(
				Diagnostic.Create(Rule, numberSyntax.GetLocation(), message));
		}
	}
}