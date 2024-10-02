using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using VaettirNet.CodeGeneration.DelegateImplementation;

namespace DelegateImplementation.Tests;

public class Tests
{
    [Test]
    public void PassThru()
    {
        IThing t = new TestThing(new DelegateTo());
        t.PassThru("TEST").Should().Be("DelegateTo.PassThru:TEST");
    }
    [Test]
    public void Overridden()
    {
        IThing t = new TestThing(new DelegateTo());
        t.Overridden().Should().Be("TestThing.Overridden");
    }
}

public interface IPizza
{
    int Toppings(int a, int b);
    string Describe();
}

public class Pizza : IPizza
{
    public int Toppings(int a, int b)
    {
        Console.WriteLine($"Pizza.Toppings({a}, {b})");
        return a + b;
    }

    public string Describe()
    {
        Console.WriteLine("Pizza.Describe()");
        return "Pizza";
    }
}

public interface IThing
{
    string PassThru(string a);
    string Overridden();
}

public abstract class BaseThing
{
    public abstract void DoIt();
}

public class DelegateTo : IThing
{
    public string PassThru(string a) => "DelegateTo.PassThru:" + a;

    public string Overridden() => "DelegateTo.Overridden";
}

public partial class TestThing : IThing
{
    [DelegateImplementation(ImplementExplicitly = true)] private readonly IThing _pizza;

    public TestThing(IThing pizza)
    {
        _pizza = pizza;
    }

    public string Overridden() => "TestThing.Overridden";
}

public class BaseVirt
{
    public virtual string Get() => "BaseVirt";
}

public partial class DerivedVirt : BaseVirt
{
    [DelegateImplementation(IncludeVirtual = true)]
    private BaseVirt _b;

    public DerivedVirt(BaseVirt b)
    {
        _b = b;
    }
}

public partial class NoDisposeStream : Stream {
    [DelegateImplementation(IncludeVirtual = true)] private readonly Stream _baseStream;
    public NoDisposeStream(Stream baseStream) => _baseStream = baseStream;
    public override void Close() { }
}