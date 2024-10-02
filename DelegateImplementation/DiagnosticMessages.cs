using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

[SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking")]
internal static class DiagnosticMessages
{
    public static readonly DiagnosticDescriptor Type0DoesNotImplement1 = new("DI0001",
        "Containing type does not implement specified type",
        "Type {0} does not implement interface {1}",
        "DelegatedImplementation",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor Field0DoesNotImplement = new("DI0002",
        "Field contains no matching interfaces of containing type",
        "Field {0} does not implement any matching interfaces in containing class",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor NoMembersDelegatedTo0 = new("DI0003",
        "No members were delegated to a field",
        "No members were delegated to field {0}, because containing class implements all members, consider removing [DelegateImplementation]",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor Fields0And1Of2BothImplement3 = new("DI0005",
        "Multiple fields delegate the same type",
        "Fields {0} and {1} of {2} both implement {3}, specify target types in [DelegateImplementation]",
        "DelegatedImplementation",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor Field0VirtualConflict = new("DI0006",
        "Field has conflicting IncludeVirtual",
        "Field {0} has conflicting values of IncludeVirtual, remove one",
        "DelegatedImplementation",
        DiagnosticSeverity.Error,
        true);

    public static readonly DiagnosticDescriptor Attribute0HasNoBaseClassTypes = new("DI0007",
        "Field has IncludeVirtual but no virtual members",
        "Field {0} specifies types and IncludeVirtual=true, but only contains interfaces, remove IncludeVirtual or add a class type",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor Field0HasNoBaseClassTypes = new("DI0008",
        "Field has IncludeVirtual but no virtual members",
        "Field {0} specifies IncludeVirtual=true, but only contains interfaces, remove IncludeVirtual or add a class type",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor Field0IsNullable = new("DI0009",
        "Delegated implementation field is nullable",
        "Field {0} is nullable, delegated implementations may throw NullReferenceExceptions",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static readonly DiagnosticDescriptor Attribute0HasNoInterfaceTypes = new("DI0010",
        "Field has ImplementExplicitly but no interface types",
        "Field {0} specifies types and ImplementExplicitly=true, but only no interfaces, remove IncludeVirtual or add an interface type",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);

    public static DiagnosticDescriptor Field0ExplicitConflict = new("DI0011",
        "Field has conflicting ImplementExplicitly",
        "Field {0} has conflicting values of ImplementExplicitly, remove one",
        "DelegatedImplementation",
        DiagnosticSeverity.Error,
        true);

    public static DiagnosticDescriptor Field0HasInterfaceTypes = new("DI0012",
        "Field has ImplementExplicitly but no interface types",
        "Field {0} specifies ImplementExplicitly=true, but only not interfaces, remove ImplementExplicitly or add an interface type",
        "DelegatedImplementation",
        DiagnosticSeverity.Warning,
        true);
}