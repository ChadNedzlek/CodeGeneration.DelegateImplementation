using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

[SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1024:Symbols should be compared for equality")]
public class TypeImplementationTargets
{
    private readonly Dictionary<ITypeSymbol, VariableDeclaratorSyntax> _explicit = new();
    private readonly HashSet<ITypeSymbol> _implementExplicitly = new();
    private readonly Dictionary<ITypeSymbol, VariableDeclaratorSyntax> _implicit = new();
    private readonly HashSet<ITypeSymbol> _includeVirtual = new();

    public IReadOnlyList<ImplTarget> GetTypesForField(VariableDeclaratorSyntax field)
    {
        return _explicit.Where(p => p.Value.Equals(field))
            .Select(p => p.Key)
            .Concat(_implicit.Where(p => p.Value.Equals(field)).Select(p => p.Key))
            .Select(t => new ImplTarget(t, _includeVirtual.Contains(t), _implementExplicitly.Contains(t)))
            .ToImmutableArray();
    }

    public bool TrySetTargetType(ITypeSymbol type,
        VariableDeclaratorSyntax target,
        bool isExplicit,
        bool includeVirtual,
        bool implementExplicitly,
        out VariableDeclaratorSyntax conflict)
    {
        if (_explicit.TryGetValue(type, out VariableDeclaratorSyntax existingExplicit))
        {
            if (isExplicit)
            {
                conflict = existingExplicit;
                return false;
            }

            // The existing one is explicit, so keep it
            conflict = null;
            return true;
        }

        if (_implicit.TryGetValue(type, out VariableDeclaratorSyntax existingImplicit))
        {
            if (isExplicit)
            {
                _includeVirtual.Remove(type);
                _implicit.Remove(type);
                _explicit.Add(type, target);
                if (includeVirtual)
                {
                    _includeVirtual.Add(type);
                }

                if (implementExplicitly)
                {
                    _implementExplicitly.Add(type);
                }

                conflict = null;
                return true;
            }

            conflict = existingImplicit;
            return false;
        }

        if (isExplicit)
        {
            _explicit.Add(type, target);
        }
        else
        {
            _implicit.Add(type, target);
        }

        if (includeVirtual)
        {
            _includeVirtual.Add(type);
        }

        if (implementExplicitly)
        {
            _implementExplicitly.Add(type);
        }

        conflict = null;
        return true;
    }
}