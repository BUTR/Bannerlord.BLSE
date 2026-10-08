using Bannerlord.BLSE.Features.AssemblyResolver;

using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BLSE.Tests;

/// <summary>
/// An AppDomain whose private bin path is <see cref="LoadContextDirectory"/>, so <c>Assembly.Load</c> of a file
/// placed there lands in the Load context, and the resolver treats it as the game's bin. Unloaded on dispose, so nothing leaks
/// into the test runner's domain.
/// </summary>
internal sealed class AssemblyResolverScenario : IDisposable
{
    private readonly AppDomain _domain;
    private readonly string _root;

    public string LoadContextDirectory { get; }

    public ScenarioRunner Runner { get; }

    public AssemblyResolverScenario()
    {
        var baseDirectory = Path.GetDirectoryName(typeof(AssemblyResolverScenario).Assembly.Location)!;
        var relativeRoot = Path.Combine("AssemblyResolverScenarios", Guid.NewGuid().ToString("N"));
        _root = Path.Combine(baseDirectory, relativeRoot);
        LoadContextDirectory = Path.Combine(_root, "LoadContext");
        Directory.CreateDirectory(LoadContextDirectory);

        var setup = new AppDomainSetup
        {
            ApplicationBase = baseDirectory,
            PrivateBinPath = Path.Combine(relativeRoot, "LoadContext"),
        };
        _domain = AppDomain.CreateDomain($"AssemblyResolverScenario_{Guid.NewGuid():N}", null, setup);
        Runner = (ScenarioRunner) _domain.CreateInstanceAndUnwrap(typeof(ScenarioRunner).Assembly.FullName, typeof(ScenarioRunner).FullName);
    }

    public string CreateDirectory(string name)
    {
        var path = Path.Combine(_root, name);
        Directory.CreateDirectory(path);
        return path;
    }

    public void Dispose()
    {
        AppDomain.Unload(_domain);
        try
        {
            Directory.Delete(_root, true);
        }
        catch (Exception)
        {
            // Best effort, the files may still be mapped
        }
    }
}

/// <summary>
/// Runs inside the scenario's AppDomain. Returns only strings, so no assembly crosses back into the runner's domain.
/// </summary>
internal sealed class ScenarioRunner : MarshalByRefObject
{
    private string[] _moduleDirectories = [];

    /// <summary>Loads into the Load context by display name and returns the location.</summary>
    public string Load(string assemblyName) => Assembly.Load(assemblyName).Location;

    /// <summary>Installs the resolver the way BLSE does, over the given directories in that order.</summary>
    public void InstallResolver(string[] moduleDirectories)
    {
        _moduleDirectories = moduleDirectories;
        AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
        {
            try
            {
                return AssemblyResolverCore.Resolve(new AssemblyName(args.Name), _moduleDirectories);
            }
            catch (Exception)
            {
                return null;
            }
        };
    }

    /// <summary>Calls the resolver directly and returns the location of what it answered, or null.</summary>
    public string? Resolve(string assemblyName, string[] moduleDirectories) =>
        AssemblyResolverCore.Resolve(new AssemblyName(assemblyName), moduleDirectories)?.Location;

    /// <summary>The locations of the loaded assemblies with this identity.</summary>
    public string[] GetLoaded(string fullName) => AppDomain.CurrentDomain.GetAssemblies()
        .Where(x => x.FullName == fullName)
        .Select(x => x.Location)
        .ToArray();

    /// <summary>The full names of the loaded assemblies with this simple name.</summary>
    public string[] GetLoadedNames(string simpleName) => AppDomain.CurrentDomain.GetAssemblies()
        .Where(x => x.GetName().Name == simpleName)
        .Select(x => x.FullName)
        .ToArray();

    public override object? InitializeLifetimeService() => null;
}
