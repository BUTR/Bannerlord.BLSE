using System.Reflection;

namespace Bannerlord.LauncherEx.Helpers;

internal class AssemblyWrapper : Assembly
{
    public override string Location { get; }

    public AssemblyWrapper(string location) => Location = location;
}