using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality")]
public class AllTypeImplementationTargets
{
    private readonly Dictionary<ITypeSymbol, TypeImplementationTargets> _targets = new();

    public TypeImplementationTargets GetForType(INamedTypeSymbol containingClass)
    {
        if (!_targets.TryGetValue(containingClass, out TypeImplementationTargets types))
        {
            _targets.Add(containingClass, types = new TypeImplementationTargets());
        }

        return types;
    }
}