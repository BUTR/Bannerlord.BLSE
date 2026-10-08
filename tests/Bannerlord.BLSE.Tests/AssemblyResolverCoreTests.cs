using NUnit.Framework;

using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;

namespace Bannerlord.BLSE.Tests;

public class AssemblyResolverCoreTests
{
    private static readonly Version V05 = new(0, 5, 0, 0);
    private static readonly Version V1 = new(1, 0, 0, 0);
    private static readonly Version V15 = new(1, 5, 0, 0);
    private static readonly Version V2 = new(2, 0, 0, 0);

    private string _fixtures = null!;
    private string _publicKeyToken = null!;

    [OneTimeSetUp]
    public void GenerateAssemblies()
    {
        var baseDirectory = Path.GetDirectoryName(typeof(AssemblyResolverCoreTests).Assembly.Location)!;
        _fixtures = Path.Combine(baseDirectory, "AssemblyResolverFixtures", Guid.NewGuid().ToString("N"));

        // Strong-named, so the identity includes the public key token as it does for System.Numerics.Vectors
        // An .snk is a CSP private key blob of a signature key
        var cspParameters = new CspParameters { KeyNumber = (int) KeyNumber.Signature, Flags = CspProviderFlags.CreateEphemeralKey };
        using var rsa = new RSACryptoServiceProvider(1024, cspParameters) { PersistKeyInCsp = false };

        foreach (var (simpleName, version) in new[] { ("X", V1), ("X", V15), ("X", V2), ("Y", V1), ("Z", V1), ("Z", V2) })
            StrongNamedAssemblyWriter.Write(Fixture(simpleName, version), simpleName, version, rsa);
        foreach (var culture in new[] { "de", "fr" })
            StrongNamedAssemblyWriter.Write(Path.Combine(_fixtures, $"R_{culture}", "R.dll"), "R", V1, rsa, culture);

        var token = AssemblyName.GetAssemblyName(Fixture("X", V1)).GetPublicKeyToken()!;
        _publicKeyToken = BitConverter.ToString(token).Replace("-", string.Empty).ToLowerInvariant();
    }

    [OneTimeTearDown]
    public void DeleteAssemblies()
    {
        try
        {
            Directory.Delete(_fixtures, true);
        }
        catch (Exception)
        {
            // Best effort
        }
    }

    // The reported crash: X v1 is loaded from the game's bin, a module's bin has a byte-identical X v1,
    // and someone requests X v2, which nobody ships.
    [Test]
    public void Request_for_another_version_returns_the_loaded_copy_instead_of_loading_an_identical_file()
    {
        using var scenario = new AssemblyResolverScenario();
        CopyTo(Fixture("X", V1), scenario.LoadContextDirectory);
        var moduleBin = scenario.CreateDirectory("Module");
        CopyTo(Fixture("X", V1), moduleBin);

        var loaded = scenario.Runner.Load(DisplayName("X", V1));
        Assert.That(loaded, Is.SamePath(Path.Combine(scenario.LoadContextDirectory, "X.dll")));

        // The bind for v2 fails, so this goes through AssemblyResolve
        scenario.Runner.InstallResolver([moduleBin]);
        var resolved = scenario.Runner.Load(DisplayName("X", V2));

        Assert.That(resolved, Is.SamePath(loaded));
        Assert.That(scenario.Runner.GetLoaded(DisplayName("X", V1)), Is.EquivalentTo(new[] { loaded }));
    }

    // Same as above, but the module ships yet another version (ButterLib's 4.1.4.0 next to the game's 4.1.3.0).
    // Loading it would put a second X in the process, so the loaded one wins.
    [Test]
    public void Request_for_another_version_prefers_the_loaded_copy_over_a_different_version_on_disk()
    {
        using var scenario = new AssemblyResolverScenario();
        CopyTo(Fixture("X", V1), scenario.LoadContextDirectory);
        var moduleBin = scenario.CreateDirectory("Module");
        CopyTo(Fixture("X", V15), moduleBin);

        var loaded = scenario.Runner.Load(DisplayName("X", V1));

        var resolved = scenario.Runner.Resolve(DisplayName("X", V2), [moduleBin]);

        Assert.That(resolved, Is.SamePath(loaded));
        Assert.That(scenario.Runner.GetLoadedNames("X"), Is.EquivalentTo(new[] { DisplayName("X", V1) }));
    }

    // A loaded copy wins even over the requested version on disk. The requester may fail later,
    // but a second X would split the types for everyone else.
    [Test]
    public void Loaded_copy_wins_over_the_requested_version_on_disk()
    {
        using var scenario = new AssemblyResolverScenario();
        CopyTo(Fixture("X", V1), scenario.LoadContextDirectory);
        var moduleBin = scenario.CreateDirectory("Module");
        CopyTo(Fixture("X", V2), moduleBin);

        var loaded = scenario.Runner.Load(DisplayName("X", V1));

        var resolved = scenario.Runner.Resolve(DisplayName("X", V2), [moduleBin]);

        Assert.That(resolved, Is.SamePath(loaded));
        Assert.That(scenario.Runner.GetLoadedNames("X"), Is.EquivalentTo(new[] { DisplayName("X", V1) }));
    }

    // The game hasn't loaded its X v1 yet when a module requests X v2. Loading the module's copy would give
    // a second copy as soon as the game binds its own, so the game's file is loaded instead.
    [Test]
    public void Identity_the_game_ships_is_loaded_from_the_game_directory_even_before_the_game_loads_it()
    {
        using var scenario = new AssemblyResolverScenario();
        CopyTo(Fixture("X", V1), scenario.LoadContextDirectory);
        var moduleBin = scenario.CreateDirectory("Module");
        CopyTo(Fixture("X", V1), moduleBin);

        var resolved = scenario.Runner.Resolve(DisplayName("X", V2), [moduleBin]);
        var gameBind = scenario.Runner.Load(DisplayName("X", V1));

        Assert.That(resolved, Is.SamePath(Path.Combine(scenario.LoadContextDirectory, "X.dll")));
        Assert.That(gameBind, Is.SamePath(resolved));
        Assert.That(scenario.Runner.GetLoaded(DisplayName("X", V1)), Has.Length.EqualTo(1));
    }

    // Satellite assemblies share the simple name across cultures
    [Test]
    public void Another_culture_never_matches()
    {
        using var scenario = new AssemblyResolverScenario();
        var german = scenario.CreateDirectory("German");
        var french = scenario.CreateDirectory("French");
        CopyTo(Path.Combine(_fixtures, "R_de", "R.dll"), german);
        CopyTo(Path.Combine(_fixtures, "R_fr", "R.dll"), french);

        var loadedGerman = scenario.Runner.Resolve(DisplayName("R", V1, "de"), [german]);
        Assert.That(loadedGerman, Is.SamePath(Path.Combine(german, "R.dll")));

        Assert.That(scenario.Runner.Resolve(DisplayName("R", V1, "fr"), [german]), Is.Null);
        Assert.That(scenario.Runner.Resolve(DisplayName("R", V1, "fr"), [german, french]), Is.SamePath(Path.Combine(french, "R.dll")));
    }

    [Test]
    public void Exact_full_name_match_returns_the_loaded_assembly()
    {
        using var scenario = new AssemblyResolverScenario();
        CopyTo(Fixture("X", V1), scenario.LoadContextDirectory);
        var moduleBin = scenario.CreateDirectory("Module");
        CopyTo(Fixture("X", V1), moduleBin);

        var loaded = scenario.Runner.Load(DisplayName("X", V1));

        var resolved = scenario.Runner.Resolve(DisplayName("X", V1), [moduleBin]);

        Assert.That(resolved, Is.SamePath(loaded));
        Assert.That(scenario.Runner.GetLoaded(DisplayName("X", V1)), Has.Length.EqualTo(1));
    }

    [Test]
    public void Not_loaded_assembly_is_found_in_a_module_bin_and_loaded_once()
    {
        using var scenario = new AssemblyResolverScenario();
        var firstModuleBin = scenario.CreateDirectory("First");
        var secondModuleBin = scenario.CreateDirectory("Second");
        CopyTo(Fixture("Y", V1), firstModuleBin);
        CopyTo(Fixture("Y", V1), secondModuleBin);

        var first = scenario.Runner.Resolve(DisplayName("Y", V1), [firstModuleBin, secondModuleBin]);
        var second = scenario.Runner.Resolve(DisplayName("Y", V1), [secondModuleBin, firstModuleBin]);

        Assert.That(first, Is.SamePath(Path.Combine(firstModuleBin, "Y.dll")));
        Assert.That(second, Is.SamePath(first));
        Assert.That(scenario.Runner.GetLoaded(DisplayName("Y", V1)), Has.Length.EqualTo(1));
    }

    [Test]
    public void File_with_a_matching_name_but_another_assembly_inside_is_ignored()
    {
        using var scenario = new AssemblyResolverScenario();
        var moduleBin = scenario.CreateDirectory("Module");
        File.Copy(Fixture("X", V1), Path.Combine(moduleBin, "Y.dll"));

        var resolved = scenario.Runner.Resolve(DisplayName("Y", V1), [moduleBin]);

        Assert.That(resolved, Is.Null);
        Assert.That(scenario.Runner.GetLoadedNames("X"), Is.Empty);
    }

    // The launcher scans Harmony's module first so its copies win over older ones shipped by other mods.
    // That relies on the first directory winning when candidates match equally well.
    [TestCase(false, "First")]
    [TestCase(true, "Second")]
    public void Within_the_same_match_quality_the_first_directory_wins(bool reverse, string expected)
    {
        using var scenario = new AssemblyResolverScenario();
        var first = scenario.CreateDirectory("First");
        var second = scenario.CreateDirectory("Second");
        CopyTo(Fixture("Z", V1), first);
        CopyTo(Fixture("Z", V2), second);

        // Both versions satisfy 0.5
        var resolved = scenario.Runner.Resolve(DisplayName("Z", V05), reverse ? [second, first] : [first, second]);

        Assert.That(resolved, Is.SamePath(Path.Combine(expected == "First" ? first : second, "Z.dll")));
        Assert.That(scenario.Runner.GetLoadedNames("Z"), Has.Length.EqualTo(1));
    }

    [TestCase("1.5.0.0", "Second")] // only v2 satisfies the requested version
    [TestCase("2.0.0.0", "Second")] // exact
    [TestCase("1.0.0.0", "First")] // exact
    [TestCase("3.0.0.0", "First")] // nothing satisfies, so the search order decides
    public void Better_match_wins_over_search_order(string requestedVersion, string expected)
    {
        using var scenario = new AssemblyResolverScenario();
        var first = scenario.CreateDirectory("First");
        var second = scenario.CreateDirectory("Second");
        CopyTo(Fixture("Z", V1), first);
        CopyTo(Fixture("Z", V2), second);

        var resolved = scenario.Runner.Resolve(DisplayName("Z", Version.Parse(requestedVersion)), [first, second]);

        Assert.That(resolved, Is.SamePath(Path.Combine(expected == "First" ? first : second, "Z.dll")));
        Assert.That(scenario.Runner.GetLoadedNames("Z"), Has.Length.EqualTo(1));
    }

    private string Fixture(string simpleName, Version version) => Path.Combine(_fixtures, $"{simpleName}_{version}", $"{simpleName}.dll");

    private string DisplayName(string simpleName, Version version, string culture = "neutral") => $"{simpleName}, Version={version}, Culture={culture}, PublicKeyToken={_publicKeyToken}";

    private static void CopyTo(string file, string directory) => File.Copy(file, Path.Combine(directory, Path.GetFileName(file)));
}
