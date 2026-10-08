using Bannerlord.BLSE.Features.AssemblyResolver;

using NUnit.Framework;

namespace Bannerlord.BLSE.Tests;

public class ModuleIdsParserTests
{
    [Test]
    public void Module_list_as_its_own_argument() => Assert.That(
        AssemblyResolverCore.ParseModuleIds(["/singleplayer", "_MODULES_*Bannerlord.Harmony*Native*SandBoxCore*_MODULES_"]),
        Is.EqualTo(new[] { "Bannerlord.Harmony", "Native", "SandBoxCore" }));

    // The vanilla launcher appends its additional arguments as one element
    [Test]
    public void Module_list_inside_an_argument() => Assert.That(
        AssemblyResolverCore.ParseModuleIds(["/no_watchdog", "/singleplayer _MODULES_*Native*SandBoxCore*_MODULES_ /continuesave save"]),
        Is.EqualTo(new[] { "Native", "SandBoxCore" }));

    [Test]
    public void Module_id_with_a_space() => Assert.That(
        AssemblyResolverCore.ParseModuleIds(["_MODULES_*Native*My", "Mod*_MODULES_"]),
        Is.EqualTo(new[] { "Native", "My Mod" }));

    [TestCase]
    [TestCase("/singleplayer")]
    [TestCase("_MODULES_*_MODULES_")]
    [TestCase("_MODULES_*Native")]
    public void No_module_list(params string[] args) =>
        Assert.That(AssemblyResolverCore.ParseModuleIds(args), Is.Null);
}
