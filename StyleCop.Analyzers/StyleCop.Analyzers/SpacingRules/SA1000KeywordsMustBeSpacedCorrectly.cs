﻿namespace StyleCop.Analyzers.SpacingRules
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The spacing around a C# keyword is incorrect.
    /// </summary>
    /// <remarks>
    /// <para>A violation of this rule occurs when the spacing around a keyword is incorrect.</para>
    ///
    /// <para>The following C# keywords must always be followed by a single space: <strong>catch</strong>,
    /// <strong>fixed</strong>, <strong>for</strong>, <strong>foreach</strong>, <strong>from</strong>,
    /// <strong>group</strong>, <strong>if</strong>, <strong>in</strong>, <strong>into</strong>, <strong>join</strong>,
    /// <strong>let</strong>, <strong>lock</strong>, <strong>orderby</strong>, <strong>return</strong>,
    /// <strong>select</strong>, <strong>stackalloc</strong>, <strong>switch</strong>, <strong>throw</strong>,
    /// <strong>using</strong>, <strong>where</strong>, <strong>while</strong>, <strong>yield</strong>.</para>
    ///
    /// <para>The following keywords must not be followed by any space: <strong>checked</strong>,
    /// <strong>default</strong>, <strong>sizeof</strong>, <strong>typeof</strong>, <strong>unchecked</strong>.</para>
    ///
    /// <para>The <strong>new</strong> keyword should always be followed by a space, unless it is used to create a new
    /// array, in which case there should be no space between the new keyword and the opening array bracket.</para>
    /// </remarks>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class SA1000KeywordsMustBeSpacedCorrectly : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SA1000";
        private const string Title = "Keywords must be spaced correctly";
        private const string MessageFormat = "The keyword '{0}' must{1} be followed by a space.";
        private const string Category = "StyleCop.CSharp.SpacingRules";
        private const string Description = "The spacing around a C# keyword is incorrect.";
        private const string HelpLink = "http://www.stylecop.com/docs/SA1000.html";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> _supportedDiagnostics =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return _supportedDiagnostics;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            // handle everything except nameof
            context.RegisterSyntaxTreeAction(HandleSyntaxTree);

            // handle nameof (which appears as an invocation expression??)
            context.RegisterSyntaxNodeAction(HandleInvocationExpressionSyntax, SyntaxKind.InvocationExpression);
        }

        private void HandleSyntaxTree(SyntaxTreeAnalysisContext context)
        {
            SyntaxNode root = context.Tree.GetCompilationUnitRoot(context.CancellationToken);
            foreach (var token in root.DescendantTokens())
            {
                switch (token.CSharpKind())
                {
                case SyntaxKind.AwaitKeyword:
                case SyntaxKind.CaseKeyword:
                case SyntaxKind.CatchKeyword:
                case SyntaxKind.FixedKeyword:
                case SyntaxKind.ForKeyword:
                case SyntaxKind.ForEachKeyword:
                case SyntaxKind.FromKeyword:
                case SyntaxKind.GroupKeyword:
                case SyntaxKind.IfKeyword:
                case SyntaxKind.InKeyword:
                case SyntaxKind.IntoKeyword:
                case SyntaxKind.JoinKeyword:
                case SyntaxKind.LetKeyword:
                case SyntaxKind.LockKeyword:
                case SyntaxKind.OrderByKeyword:
                case SyntaxKind.SelectKeyword:
                case SyntaxKind.StackAllocKeyword:
                case SyntaxKind.SwitchKeyword:
                case SyntaxKind.UsingKeyword:
                case SyntaxKind.WhereKeyword:
                case SyntaxKind.WhileKeyword:
                case SyntaxKind.YieldKeyword:
                    HandleRequiredSpaceToken(context, token);
                    break;

                case SyntaxKind.CheckedKeyword:
                case SyntaxKind.DefaultKeyword:
                case SyntaxKind.NameOfKeyword:
                case SyntaxKind.SizeOfKeyword:
                case SyntaxKind.TypeOfKeyword:
                case SyntaxKind.UncheckedKeyword:
                    HandleDisallowedSpaceToken(context, token);
                    break;

                case SyntaxKind.NewKeyword:
                    HandleNewKeywordToken(context, token);
                    break;

                case SyntaxKind.ReturnKeyword:
                    HandleReturnKeywordToken(context, token);
                    break;

                case SyntaxKind.ThrowKeyword:
                    HandleThrowKeywordToken(context, token);
                    break;

                default:
                    break;
                }
            }
        }

        private void HandleInvocationExpressionSyntax(SyntaxNodeAnalysisContext context)
        {
            InvocationExpressionSyntax invocationExpressionSyntax = (InvocationExpressionSyntax)context.Node;
            IdentifierNameSyntax identifierNameSyntax = invocationExpressionSyntax.Expression as IdentifierNameSyntax;
            if (identifierNameSyntax == null)
                return;

            if (identifierNameSyntax.Identifier.IsMissing)
                return;

            if (identifierNameSyntax.Identifier.Text != "nameof")
                return;

            var constantValue = context.SemanticModel.GetConstantValue(invocationExpressionSyntax, context.CancellationToken);
            if (constantValue.HasValue && !string.IsNullOrEmpty(constantValue.Value as string))
            {
                // this is a nameof expression
                HandleDisallowedSpaceToken(context, identifierNameSyntax.Identifier);
            }
        }

        private void HandleRequiredSpaceToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing)
                return;

            if (token.HasTrailingTrivia)
            {
                if (token.TrailingTrivia.First().IsKind(SyntaxKind.WhitespaceTrivia))
                    return;

                if (token.TrailingTrivia.First().IsKind(SyntaxKind.EndOfLineTrivia))
                    return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), token.Text, string.Empty));
        }

        private void HandleDisallowedSpaceToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing || !token.HasTrailingTrivia)
                return;

            if (!token.TrailingTrivia.First().IsKind(SyntaxKind.WhitespaceTrivia))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), token.Text, " not"));
        }

        private void HandleDisallowedSpaceToken(SyntaxNodeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing || !token.HasTrailingTrivia)
                return;

            if (!token.TrailingTrivia.First().IsKind(SyntaxKind.WhitespaceTrivia))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, token.GetLocation(), token.Text, " not"));
        }

        private void HandleNewKeywordToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing)
                return;

            // if the next token is [ or (, then treat as disallowed
            SyntaxToken nextToken = token.GetNextToken();
            if (nextToken.IsKind(SyntaxKind.OpenBracketToken) || nextToken.IsKind(SyntaxKind.OpenParenToken))
            {
                if (token.Parent.IsKind(SyntaxKind.ImplicitArrayCreationExpression))
                {
                    // This is handled by SA1026
                    return;
                }

                HandleDisallowedSpaceToken(context, token);
                return;
            }

            // otherwise treat as required
            HandleRequiredSpaceToken(context, token);
        }

        private void HandleReturnKeywordToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing)
                return;

            // if the next token is ; or :, then treat as disallowed
            //   1. return;
            //   2. [return: Attribute(...)]
            SyntaxToken nextToken = token.GetNextToken();
            if (nextToken.IsKind(SyntaxKind.SemicolonToken) || nextToken.IsKind(SyntaxKind.ColonToken))
            {
                HandleDisallowedSpaceToken(context, token);
                return;
            }

            // otherwise treat as required
            HandleRequiredSpaceToken(context, token);
        }

        private void HandleThrowKeywordToken(SyntaxTreeAnalysisContext context, SyntaxToken token)
        {
            if (token.IsMissing)
                return;

            // if the next token is ;, then treat as disallowed:
            //    throw;
            SyntaxToken nextToken = token.GetNextToken();
            if (nextToken.IsKind(SyntaxKind.SemicolonToken))
            {
                HandleDisallowedSpaceToken(context, token);
                return;
            }

            // otherwise treat as required
            HandleRequiredSpaceToken(context, token);
        }
    }
}
