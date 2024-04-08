using HarmonyLib.BUTR.Extensions;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Bannerlord.BLSE;

public static class BLSECommands
{
    public static string GetVersion(List<string> _)
    {
        var blseMetadata = AccessTools2.TypeByName("Bannerlord.BLSE.BLSEInterceptorAttribute")?.Assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
        return blseMetadata?.FirstOrDefault(x => x.Key == "BLSEVersion")?.Value ?? "0.0.0.0";
    }
}