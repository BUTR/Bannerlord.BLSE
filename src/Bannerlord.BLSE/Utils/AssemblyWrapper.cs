using System.Reflection;

namespace Bannerlord.BLSE.Utils;

internal class AssemblyWrapper : Assembly
{
    public override string Location { get; }

    public AssemblyWrapper(string location) => Location = location;
}