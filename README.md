## CodeGeneration.DelegateImplementation
### Summary
A C# code generator that will generate delegating methods automatically for you that delegate implementations to a field (or fields for complex configurations).

There are often times when a class is wrapping some underlying implementation and only needs to slightly modify behavior (perhaps of only one or two methods), and having to manually write delegating methods can be cumbersome and error prone (accidentally copy/pasting the wrong inner call, for example).

System.IO.Stream is a particularly useful case, as there are dozens of methods that need to be overridden/implemented for optimal behavior.

### Basic Usage
```csharp
interface ISample { int GetValue(string input); }

partial class SampleClass : ISample {
	[DelegateImplementation] private ISample _inner;
	
	public SampleClass(ISample inner) => _inner = inner;
}
```

this will produce generated members that look something like the following
```csharp
partial class SampleClass {
    public int GetValue(string input) => _inner.GetValue(input);
}
```
### Advanced Usage
#### Multiple fields
Delegation is performed for all types that the containing class and the field share in common, which means multiple delegations can be done by having fields with different overlapping types.
```csharp
interface IFirst { int First(); }
interface ISecond { int Second(); }

partial class SampleClass : IFirst, ISecond {
	[DelegateImplementation] private IFirst _first;
	[DelegateImplementation] private ISecond _second;
}
```

#### Partial Implementations
If the implementing class already provides implementations for a particular method, that method will not be generated, and the provided implementation will be used.
##### Example
```csharp
interface ISample {
	int FirstMethod();
	int SecondMethod();
}

partial class SampleClass : ISample {
	[DelegateImplementation] private ISample _inner;

	public int FirstMethod() => 7;
}
```
the generated class with only produce an implementation for `SecondMethod`

#### Specific types
Specific types can be passed to the attribute, which will cause it to only generate methods for that type and not others.  This can be useful when delegating to multiple fields with overlapping types to specify which will be handled.
```csharp
interface IFirst { int First(); }
interface ISecond : IFirst { int Second(); }

partial class SampleClass : IFirst, ISecond {
	[DelegateImplementation] private IFirst _first;
	[DelegateImplementation(typeof(ISecond))] private ISecond _second;
}
```

Without the specified type, this would produce an error,
because the `IFirst` interface can be handled by multiple fields,
so it's necessary to mark `_second` as only delegating the `ISecond` implementation.

#### Explicit Implementations
Normally all implemented methods are implemented not explicitly (as `public ...()` methods) except where necessary
(for example, when delegating `IEnumerable<T>` there are conflicting `GetEnumeator` methods, in which case
the generator attempts to generate the "best" method publicly, and the others explicitly.

However this behavior can be controlled with the `ImplementExplicitly` argument.

```csharp
interface ISample { int GetValue(string input); }

partial class SampleClass : ISample {
	[DelegateImplementation(ImplementExplicitly = true)] private ISample _inner;	
}
```

this will produce generated members that look something like the following
```csharp
partial class SampleClass {
    int ISample.GetValue(string input) => _inner.GetValue(input);
}
```

#### Implement Only Abstract / All Virtual
By default only the minimum number of methods are generated to get a correct implementation
(so only abstract members of base classes and members of interfaces.

However this behavior can be controlled with the `IncludeVirtual` argument.

```csharp
abstract class SampleBase {
	virtual int VirtGetValue(string input) => AbsGetValue() + 5;
	abstract int AbsGetValue(string input);
}

partial class SampleClass : ISample {
	[DelegateImplementation(IncludeVirtual = true)]	private SampleBase _inner;	
}
```

this will produce generated members that look something like the following
```csharp
partial class SampleClass {
    int ISample.GetValue(string input) => _inner.GetValue(input);
}
```

Without the `IncludeVirtual`, only the `AbsGetValue` method would get delegated.
With it true, it will generate `VirtGetValue` as well.
However, it can be beneficial to also include all virtual members as well.
For example, System.IO.Stream has many virtual methods that are not necessary to override,
but if not overridden, the base implementations will perform very poorly (having to allocate temporary arrays and copying values in and out),
and it's preferable to also implement/delegate the virtual methods as well.
