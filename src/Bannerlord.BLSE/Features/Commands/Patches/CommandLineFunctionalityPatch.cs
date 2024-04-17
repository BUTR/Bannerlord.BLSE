using Bannerlord.BUTR.Shared.Extensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections;
using System.Collections.Generic;

using TaleWorlds.Library;

namespace Bannerlord.BLSE.Features.Commands.Patches;

internal static class CommandLineFunctionalityPatch
{
    private readonly ref struct CommandLineFunctionHandle
    {
        private delegate object CommandLineFunctionCtorDelegate(Func<List<string>, string> commandlinefunc);
        private static readonly CommandLineFunctionCtorDelegate? CommandLineFunctionCtor =
            AccessTools2.GetConstructorDelegate<CommandLineFunctionCtorDelegate>("TaleWorlds.Library.CommandLineFunctionality+CommandLineFunction", new[] { typeof(Func<List<string>, string>) });

        public static CommandLineFunctionHandle Create(Func<List<string>, string> commandlinefunc)
        {
            var commandLineFunction = CommandLineFunctionCtor?.Invoke(commandlinefunc);
            return commandLineFunction is not null ? new(commandLineFunction) : default;
        }

        public object Object { get; }

        private CommandLineFunctionHandle(object obj) => Object = obj;
    }

    private static Harmony? _harmony;

    public static bool Enable(Harmony harmony)
    {
        _harmony = harmony;

        return harmony.TryPatch(
            AccessTools2.DeclaredMethod(typeof(CommandLineFunctionality), nameof(CommandLineFunctionality.CollectCommandLineFunctions)),
            postfix: AccessTools2.DeclaredMethod(typeof(CommandLineFunctionalityPatch), nameof(CollectCommandLineFunctionsPostfix)));
    }

    private static void CollectCommandLineFunctionsPostfix(IDictionary ___AllFunctions, ref List<string> __result)
    {
        try
        {
            foreach (var (name, function) in CommandsFeature.Functions)
            {
                if (CommandLineFunctionHandle.Create(function) is not { Object: { } cmdFuncObject }) continue;
                ___AllFunctions.Add(name, cmdFuncObject);
                __result.Add(name);
            }
        }
        finally
        {
            _harmony?.Unpatch(
                AccessTools2.DeclaredMethod(typeof(CommandLineFunctionality), nameof(CommandLineFunctionality.CollectCommandLineFunctions)),
                AccessTools2.DeclaredMethod(typeof(CommandLineFunctionalityPatch), nameof(CollectCommandLineFunctionsPostfix)));
        }
    }
}