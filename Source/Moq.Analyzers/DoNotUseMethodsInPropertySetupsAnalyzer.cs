using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Moq.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotUseMethodsInPropertySetupsAnalyzer : DiagnosticAnalyzer
    {
        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            Diagnostics.MethodInPropertySetupId, 
            Diagnostics.MethodInPropertySetupTitle, 
            Diagnostics.MethodInPropertySetupMessage, 
            Diagnostics.Category, DiagnosticSeverity.Warning, isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(Analyze, SyntaxKind.InvocationExpression);
        }

        private static void Analyze(SyntaxNodeAnalysisContext context)
        {

            var setupGetOrSetInvocation = (InvocationExpressionSyntax)context.Node;

            var setupGetOrSetMethod = setupGetOrSetInvocation.Expression as MemberAccessExpressionSyntax;
            if (setupGetOrSetMethod == null) return;
            if (setupGetOrSetMethod.Name.ToFullString() != "SetupGet" && setupGetOrSetMethod.Name.ToFullString() != "SetupSet") return;

            var mockedMethodCall = Helpers.FindMockedMethodInvocationFromSetupMethod(context.SemanticModel, setupGetOrSetInvocation);
            if (mockedMethodCall == null) return;

            var mockedMethodSymbol = context.SemanticModel.GetSymbolInfo(mockedMethodCall).Symbol;
            if (mockedMethodSymbol == null) return;

            var diagnostic = Diagnostic.Create(Rule, mockedMethodCall.GetLocation());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
