using System;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Ardalis.Cli.Infrastructure;

/// <summary>
/// Bridges Spectre.Console.Cli with Microsoft.Extensions.DependencyInjection
/// </summary>
public sealed class TypeRegistrar : ITypeRegistrar
{
    private readonly IServiceProvider _provider;

    public TypeRegistrar(IServiceProvider provider)
    {
        _provider = provider;
    }

    public ITypeResolver Build()
    {
        return new TypeResolver(_provider);
    }

    public void Register(Type service, Type implementation)
    {
        // Not needed when using an existing IServiceProvider
    }

    public void RegisterInstance(Type service, object implementation)
    {
        // Not needed when using an existing IServiceProvider
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        // Not needed when using an existing IServiceProvider
    }
}

/// <summary>
/// Resolves types from the DI container
/// </summary>
public sealed class TypeResolver : ITypeResolver
{
    private readonly IServiceProvider _provider;

    public TypeResolver(IServiceProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public object Resolve(Type type)
    {
        if (type == null)
        {
            return null;
        }

        // Try to get from DI container first
        var service = _provider.GetService(type);
        if (service != null)
        {
            return service;
        }

        // Fall back to activator for types not registered in DI
        // This handles Spectre.Console.Cli internal types like EmptyCommandSettings
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            throw new InvalidOperationException($"Could not resolve type '{type.FullName}'.");
        }
    }
}
