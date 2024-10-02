using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

#pragma warning disable RS1024

[Generator]
public class DelegateImplementationGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // Register a syntax receiver that will be created for each generation pass
        context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
#pragma warning disable RS1035
        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.launchanalyzerdebugger",
                out string launch) && launch == "true")
        {
            Debugger.Launch();
        }

        // Retrieve the populated receiver
        if (context.SyntaxReceiver is not SyntaxReceiver receiver)
        {
            return;
        }

        // Get the semantic model
        Compilation compilation = context.Compilation;

        if (receiver.Fields.Count == 0)
        {
            return;
        }

        AllTypeImplementationTargets missingTypes = GetTypeTargets(context, receiver, compilation);

        var sourceBuilder = new StringBuilder("""
            #pragma warning disable
            #nullable enable annotations

            """);
        foreach (FieldDeclarationSyntax fieldDeclarationSyntax in receiver.Fields)
        {
            SemanticModel model = compilation.GetSemanticModel(fieldDeclarationSyntax.SyntaxTree);
            VariableDeclaratorSyntax field = fieldDeclarationSyntax.Declaration.Variables.First();

            if (model.GetDeclaredSymbol(field) is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            INamedTypeSymbol containingClass = fieldSymbol.ContainingType;

            if (containingClass.TypeKind != TypeKind.Class)
            {
                continue;
            }

            IReadOnlyList<ImplTarget> types = missingTypes.GetForType(containingClass).GetTypesForField(field);
            ;

            sourceBuilder.Append($$"""
                namespace {{containingClass.ContainingNamespace}}
                {
                  // Delegation for field {{fieldSymbol}}
                  [System.CodeDom.Compiler.GeneratedCode("DelegatedImplementation", "{{GetType().Assembly.GetName().Version}}")]
                  partial class {{containingClass.Name}}
                  {

                """);

            var foundMatch = false;
            var generatedMember = false;

            HashSet<ISymbol> hidden = GetHiddenMembers(containingClass);

            var fieldAccess = $"this.{field.Identifier.Text}";
            foreach (ImplTarget target in types)
            {
                ITypeSymbol targetType = target.TargetType;
                if (!IsConvertibleTo(fieldSymbol.Type, targetType))
                {
                    sourceBuilder.Append($"    // {fieldAccess} does not implement {targetType}\n");
                    continue;
                }

                sourceBuilder.Append($"    // Delegation for {targetType.TypeKind} {targetType}\n");

                foundMatch = true;

                foreach (ISymbol symbol in targetType.GetMembers())
                {
                    if (hidden.Contains(symbol))
                    {
                        continue;
                    }

                    string delegated = GetDelegatedCall(symbol,
                        targetType,
                        fieldSymbol.ContainingType,
                        fieldAccess,
                        target.IncludeVirtual,
                        target.ImplementExplicitly);
                    if (delegated != null)
                    {
                        sourceBuilder.Append("    ");
                        sourceBuilder.Append(delegated);
                        generatedMember = true;
                    }
                }
            }

            if (!foundMatch)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Field0DoesNotImplement,
                    fieldDeclarationSyntax.GetLocation(),
                    field));
            }
            else if (!generatedMember)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.NoMembersDelegatedTo0,
                    fieldDeclarationSyntax.GetLocation(),
                    field));
            }

            sourceBuilder.Append("""
                  }
                }

                """);
        }

        context.AddSource("DelegatedImplementations.generated.cs",
            SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    private string GetDelegatedCall(ISymbol symbol,
        ITypeSymbol iface,
        ITypeSymbol containingType,
        string fieldAccess,
        bool includeVirtual,
        bool implementExplicitly)
    {
        if (!symbol.IsAbstract && !includeVirtual)
        {
            return null;
        }

        if (!symbol.IsVirtual && !symbol.IsAbstract)
        {
            return null;
        }

        IEnumerable<ITypeSymbol> matched = containingType.AllInterfaces.Except([iface])
            .SelectMany(i => i.GetMembers().Select(m => OtherReturnType(symbol, m)))
            .Where(t => t != null);

        switch (symbol)
        {
            case IMethodSymbol method:
            {
                if (method.AssociatedSymbol != null || method.DeclaredAccessibility != Accessibility.Public)
                {
                    return null;
                }

                bool declareExplicit = implementExplicitly || matched.Any(m => IsSubtypeOf(m, method.ReturnType));
                ISymbol impl = containingType.FindImplementationForInterfaceMember(method);
                if (impl != null)
                {
                    return null;
                }

                string accessibility = SyntaxFacts.GetText(method.DeclaredAccessibility);
                var methodCallString =
                    $"(({iface}){fieldAccess}).@{method.Name}({string.Join(", ", method.Parameters.Select(p => $"@{p.Name}"))})";
                var nameAndParameters =
                    $"@{method.Name}({string.Join(", ", method.Parameters.Select(p => $"{p.Type} @{p.Name}"))})";

                if (method.ContainingType.TypeKind != TypeKind.Interface)
                {
                    return $"{accessibility} override {method.ReturnType} {nameAndParameters} => {methodCallString};\n";
                }

                if (declareExplicit)
                {
                    return $"{method.ReturnType} {iface}.{nameAndParameters} => {methodCallString};\n";
                }

                return $"public {method.ReturnType} {nameAndParameters} => {methodCallString};\n";
            }
            case IPropertySymbol prop:
            {
                ISymbol impl = containingType.FindImplementationForInterfaceMember(prop);
                if (impl != null)
                {
                    return null;
                }

                if (prop.DeclaredAccessibility < Accessibility.Internal)
                {
                    return null;
                }

                bool declareExplicit = implementExplicitly || matched.Any(m => IsSubtypeOf(m, prop.Type));
                string accessibility = SyntaxFacts.GetText(prop.DeclaredAccessibility);

                string methodCallString;
                string nameAndParameters;
                if (prop.IsIndexer)
                {
                    methodCallString =
                        $"(({iface}){fieldAccess})[{string.Join(", ", prop.Parameters.Select(p => $"@{p.Name}"))}]";
                    nameAndParameters =
                        $"this[{string.Join(", ", prop.Parameters.Select(p => $"{p.Type} @{p.Name}"))}]";
                }
                else
                {
                    methodCallString = $"(({iface}){fieldAccess}).@{prop.Name}";
                    nameAndParameters = $"@{prop.Name}";
                }

                string propDecl;
                if (prop.ContainingType.TypeKind != TypeKind.Interface)
                {
                    propDecl = $"{accessibility} override {prop.Type} {nameAndParameters}";
                }
                else if (declareExplicit)
                {
                    propDecl = $"{prop.Type} {iface}.{nameAndParameters}";
                }
                else
                {
                    propDecl = $"public {prop.Type} {nameAndParameters} => {methodCallString};\n";
                }

                bool generateGet = !prop.IsWriteOnly && prop.GetMethod?.DeclaredAccessibility >= Accessibility.Internal;
                bool generateSet = !prop.IsReadOnly && prop.SetMethod?.DeclaredAccessibility >= Accessibility.Internal;

                if (generateGet)
                {
                    if (generateSet)
                    {
                        var getAccessibility = "";
                        if (prop.GetMethod != null &&
                            prop.GetMethod.DeclaredAccessibility != prop.DeclaredAccessibility)
                        {
                            getAccessibility = SyntaxFacts.GetText(prop.GetMethod.DeclaredAccessibility) + " ";
                        }

                        var setAccessibility = "";
                        if (prop.SetMethod != null &&
                            prop.SetMethod.DeclaredAccessibility != prop.DeclaredAccessibility)
                        {
                            setAccessibility = SyntaxFacts.GetText(prop.SetMethod.DeclaredAccessibility) + " ";
                        }

                        return
                            $"{propDecl} {{ {getAccessibility} get => {methodCallString}; {setAccessibility} set => {methodCallString} = value; }}\n";
                    }

                    return $"{propDecl} => {methodCallString};\n";
                }

                return $"{propDecl} {{ set => {methodCallString} = value; }}\n";
            }
        }

        return null;
    }

    private static HashSet<ISymbol> GetHiddenMembers(INamedTypeSymbol containingClass)
    {
        HashSet<ISymbol> overridden = [];
        foreach (ISymbol member in containingClass.GetMembers())
            switch (member)
            {
                case IMethodSymbol method:
                    while (method != null)
                    {
                        overridden.Add(method);
                        method = method.OverriddenMethod;
                    }

                    break;
                case IPropertySymbol property:
                    while (property != null)
                    {
                        overridden.Add(property);
                        property = property.OverriddenProperty;
                    }

                    break;
            }

        return overridden;
    }

    private static AllTypeImplementationTargets GetTypeTargets(GeneratorExecutionContext context,
        SyntaxReceiver receiver,
        Compilation compilation)
    {
        AllTypeImplementationTargets all = new();
        foreach (FieldDeclarationSyntax fieldDeclarationSyntax in receiver.Fields)
        {
            SemanticModel model = compilation.GetSemanticModel(fieldDeclarationSyntax.SyntaxTree);
            VariableDeclaratorSyntax field = fieldDeclarationSyntax.Declaration.Variables.First();
            if (model.GetDeclaredSymbol(field) is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            ITypeSymbol fieldType = fieldSymbol.Type;

            if (fieldType.NullableAnnotation == NullableAnnotation.Annotated)
            {
                Diagnostic.Create(DiagnosticMessages.Field0IsNullable, field.GetLocation(), field.Identifier);
            }

            INamedTypeSymbol containingClass = fieldSymbol.ContainingType;
            FieldForwardInfo forwardInfo = ReadAttributeInstructions(fieldDeclarationSyntax, context, model);

            TypeImplementationTargets types = all.GetForType(containingClass);
            var hasNonInterface = false;
            var hasInterface = false;
            foreach (ForwardTypeInfo typeInfo in forwardInfo.Types)
            {
                bool virtualFor = typeInfo.OverrideVirtuals ?? forwardInfo.DefaultVirtual ?? false;
                bool explicitFor = typeInfo.ImplementExplicit ?? forwardInfo.DefaultExplicit ?? false;

                hasNonInterface |= typeInfo.Target.TypeKind != TypeKind.Interface;
                hasInterface |= typeInfo.Target.TypeKind == TypeKind.Interface;

                if (!types.TrySetTargetType(typeInfo.Target,
                        field,
                        !forwardInfo.AllMatchingTypes,
                        virtualFor,
                        explicitFor,
                        out VariableDeclaratorSyntax current))
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Fields0And1Of2BothImplement3,
                        fieldDeclarationSyntax.GetLocation(),
                        [current.GetLocation()],
                        field.Identifier,
                        current.Identifier,
                        containingClass,
                        typeInfo.Target));
                }
            }

            if (!hasNonInterface && forwardInfo.DefaultVirtual.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Field0HasNoBaseClassTypes,
                    field.GetLocation(),
                    field.Identifier));
            }

            if (!hasInterface && forwardInfo.DefaultExplicit.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Field0HasInterfaceTypes,
                    field.GetLocation(),
                    field.Identifier));
            }
        }

        return all;
    }

    private record ForwardTypeInfo(ITypeSymbol Target, bool? ImplementExplicit, bool? OverrideVirtuals);

    private record FieldForwardInfo(
        IReadOnlyList<ForwardTypeInfo> Types,
        bool? DefaultExplicit,
        bool? DefaultVirtual,
        bool AllMatchingTypes);

    private static FieldForwardInfo ReadAttributeInstructions(
        FieldDeclarationSyntax fieldDeclarationSyntax,
        GeneratorExecutionContext context,
        SemanticModel model)
    {
        VariableDeclaratorSyntax field = fieldDeclarationSyntax.Declaration.Variables.First();
        var fieldSymbol = (IFieldSymbol)model.GetDeclaredSymbol(field);
        INamedTypeSymbol containingClass = fieldSymbol.ContainingType;
        ITypeSymbol fieldType = fieldSymbol.Type;
        List<INamedTypeSymbol> specificTypes = [];
        Dictionary<ITypeSymbol, bool> includeVirtualFor = new();
        Dictionary<ITypeSymbol, bool> includeExplicitFor = new();
        bool? defaultVirtual = null;
        bool? defaultExplicit = null;
        foreach (AttributeSyntax attribute in fieldDeclarationSyntax.AttributeLists.SelectMany(l => l.Attributes))
        {
            bool? virtualSpecified =
                GetSpecifiedValue(attribute, nameof(DelegateImplementationAttribute.IncludeVirtual), model);
            bool? explicitSpecified = GetSpecifiedValue(attribute,
                nameof(DelegateImplementationAttribute.ImplementExplicitly),
                model);
            var hadTypes = false;
            var hadClass = false;
            var hadInterface = false;

            foreach (AttributeArgumentSyntax argument in attribute.ArgumentList?.Arguments ?? [])
                if (argument.Expression is TypeOfExpressionSyntax to)
                {
                    var targetType = (INamedTypeSymbol)model.GetSymbolInfo(to.Type).Symbol;
                    if (!IsSubtypeOf(containingClass, targetType))
                    {
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Type0DoesNotImplement1,
                            fieldDeclarationSyntax.GetLocation(),
                            fieldSymbol.ContainingType,
                            targetType));
                    }

                    if (targetType.TypeKind == TypeKind.Interface)
                    {
                        hadInterface = true;
                    }
                    else
                    {
                        hadClass = true;
                    }

                    specificTypes.Add(targetType);

                    if (virtualSpecified is bool v)
                    {
                        includeVirtualFor.Add(targetType, v);
                    }

                    if (explicitSpecified is bool e)
                    {
                        includeExplicitFor.Add(targetType, e);
                    }

                    hadTypes = true;
                }

            if (!hadClass && hadTypes && virtualSpecified.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Attribute0HasNoBaseClassTypes,
                    field.GetLocation(),
                    field.Identifier));
            }

            if (!hadInterface && hadTypes && explicitSpecified.HasValue)
            {
                context.ReportDiagnostic(Diagnostic.Create(DiagnosticMessages.Attribute0HasNoInterfaceTypes,
                    field.GetLocation(),
                    field.Identifier));
            }

            if (!hadTypes && defaultVirtual.HasValue && virtualSpecified.HasValue)
            {
                Diagnostic.Create(DiagnosticMessages.Field0VirtualConflict,
                    fieldDeclarationSyntax.GetLocation(),
                    field.Identifier);
            }

            if (!hadTypes && defaultExplicit.HasValue && explicitSpecified.HasValue)
            {
                Diagnostic.Create(DiagnosticMessages.Field0ExplicitConflict,
                    fieldDeclarationSyntax.GetLocation(),
                    field.Identifier);
            }

            if (!hadTypes)
            {
                defaultVirtual = virtualSpecified;
                defaultExplicit = explicitSpecified;
            }
        }

        IReadOnlyList<ITypeSymbol> targetTypes =
            specificTypes.Count == 0 ? AllBaseTypes(fieldType).Add(fieldType) : specificTypes;

        ImmutableList<ForwardTypeInfo> types = targetTypes.Select(t => new ForwardTypeInfo(t,
                includeExplicitFor.TryGetValue(t, out bool e) ? e : null,
                includeVirtualFor.TryGetValue(t, out bool v) ? v : null))
            .ToImmutableList();

        return new FieldForwardInfo(types, defaultExplicit, defaultVirtual, specificTypes.Count != 0);
    }

    private static bool? GetSpecifiedValue(AttributeSyntax attribute, string property, SemanticModel model)
    {
        AttributeArgumentSyntax propertyArgument = attribute.ArgumentList?.Arguments.FirstOrDefault(v =>
            v.NameEquals is { } ne &&
            ne.Name.ToString() == property);

        if (propertyArgument == null)
        {
            return null;
        }

        return model.GetOperation(propertyArgument.Expression) is { ConstantValue: { HasValue: true, Value: true } };
    }

    public static ImmutableArray<ITypeSymbol> AllBaseTypes(ITypeSymbol containingClass)
    {
        ImmutableArray<INamedTypeSymbol> types = containingClass.AllInterfaces;
        if (containingClass.BaseType != null && containingClass.BaseType.BaseType != null)
        {
            types = types.Add(containingClass.BaseType);
        }

        return types.Cast<ITypeSymbol>().ToImmutableArray();
    }

    private static bool IsConvertibleTo(ITypeSymbol sourceType, ITypeSymbol convertType)
    {
        return IsSubtypeOf(sourceType, convertType) || sourceType.Equals(convertType);
    }

    private static bool IsSubtypeOf(ITypeSymbol candidate, ITypeSymbol self)
    {
        INamedTypeSymbol b = candidate.BaseType;
        while (b != null)
        {
            if (b.Equals(self))
            {
                return true;
            }

            b = b.BaseType;
        }

        return candidate.AllInterfaces.Any(i => i.Equals(self));
    }

    private ITypeSymbol OtherReturnType(ISymbol self, ISymbol other)
    {
        if (self.Name != other.Name || self.Kind != other.Kind)
        {
            return null;
        }

        return (self, other) switch
        {
            (IMethodSymbol s, IMethodSymbol o) => ParametersMatch(s.Parameters, o.Parameters) ? o.ReturnType : null,
            (IPropertySymbol s, IPropertySymbol o) => ParametersMatch(s.Parameters, o.Parameters) ? o.Type : null,
            _ => null
        };
    }

    private bool ParametersMatch(ImmutableArray<IParameterSymbol> a, ImmutableArray<IParameterSymbol> b)
    {
        if (a.Length != b.Length)
        {
            return false;
        }

        for (var i = 0; i < a.Length; i++)
            if (!a[i].Type.Equals(b[i].Type))
            {
                return false;
            }

        return true;
    }

    private class SyntaxReceiver : ISyntaxReceiver
    {
        public List<FieldDeclarationSyntax> Fields { get; } = new();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            // Look for fields with the DeferedImplementation attribute
            if (syntaxNode is FieldDeclarationSyntax fieldDeclaration &&
                fieldDeclaration.AttributeLists.Any(al => al.Attributes.Any(a =>
                    a.Name.ToString() is "DelegateImplementationAttribute" or "DelegateImplementation")))
            {
                Fields.Add(fieldDeclaration);
            }
        }
    }
}