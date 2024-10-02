using System;

namespace VaettirNet.CodeGeneration.DelegateImplementation;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class DelegateImplementationAttribute : Attribute
{
    public DelegateImplementationAttribute()
    {
        Targets = [];
    }

    public DelegateImplementationAttribute(params Type[] targets)
    {
        Targets = targets;
    }

    public Type[] Targets { get; }
    public bool IncludeVirtual { get; set; }
    public bool ImplementExplicitly { get; set; }
}