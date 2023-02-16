using Bannerlord.BLSE.Features.Commands.Patches;

using HarmonyLib;

using System;
using System.Collections.Generic;

namespace Bannerlord.BLSE.Features.Commands
{
    public static class CommandsFeature
    {
        public static string Id = FeatureIds.CommandsId;

        public static readonly Dictionary<string, Func<List<string>, string>> Functions = new()
        {
            { "blse.version", BLSECommands.GetVersion }
        };

        public static void Enable(Harmony harmony)
        {
            CommandLineFunctionalityPatch.Enable(harmony);
        }
    }
}