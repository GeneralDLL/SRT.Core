//using System;
//using System.Collections.Generic;
//using System.Collections.Immutable;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace GeneralDLL.Core.CustomAttributes
//{
//    [DiagnosticAnalyzer(LanguageNames.CSharp)]
//    public class InjectionControllerAnalyzer : DiagnosticAnalyzer
//    {
//        public const string DiagnosticId = "IC001";
//        private static readonly LocalizableString Title = "Invalid InjectionController usage";
//        private static readonly LocalizableString MessageFormat = "Class '{0}' must implement interface '{1}'";
//        private static readonly DiagnosticDescriptor Rule =
//            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat,
//                "DependencyInjection", DiagnosticSeverity.Error, isEnabledByDefault: true);

//        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

//        public override void Initialize(AnalysisContext context)
//        {
//            context.EnableConcurrentExecution();
//            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

//            context.RegisterSymbolAction(AnalyzeClass, SymbolKind.NamedType);
//        }

//        private void AnalyzeClass(SymbolAnalysisContext context)
//        {
//            var classSymbol = (INamedTypeSymbol)context.Symbol;

//            // چک کنیم Attribute داشته باشه
//            var attributes = classSymbol.GetAttributes()
//                .Where(a => a.AttributeClass?.Name == "InjectionControllerAttribute");

//            foreach (var attr in attributes)
//            {
//                if (attr.ConstructorArguments.Length < 2) continue;

//                // ServiceTypes از پارامتر دوم به بعد هستند
//                var serviceTypes = attr.ConstructorArguments
//                    .Skip(1)
//                    .SelectMany(arg => arg.Values.Select(v => v.Value as INamedTypeSymbol))
//                    .Where(t => t != null);

//                foreach (var serviceType in serviceTypes)
//                {
//                    if (!classSymbol.AllInterfaces.Contains(serviceType))
//                    {
//                        var diag = Diagnostic.Create(Rule,
//                            classSymbol.Locations[0],
//                            classSymbol.Name,
//                            serviceType!.Name);

//                        context.ReportDiagnostic(diag);
//                    }
//                }
//            }
//        }
//    }