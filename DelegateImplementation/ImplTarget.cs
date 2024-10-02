using Microsoft.CodeAnalysis;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

public class ImplTarget
{
    public ImplTarget(ITypeSymbol targetType, bool includeVirtual, bool implementExplicitly)
    {
        TargetType = targetType;
        IncludeVirtual = includeVirtual;
        ImplementExplicitly = implementExplicitly;
    }

    public ITypeSymbol TargetType { get; }
    public bool IncludeVirtual { get; }
    public bool ImplementExplicitly { get; }
}