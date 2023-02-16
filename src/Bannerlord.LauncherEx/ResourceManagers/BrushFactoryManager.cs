using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

using TaleWorlds.GauntletUI;

namespace Bannerlord.LauncherEx.ResourceManagers
{
    internal static class BrushFactoryManager
    {
        private static readonly Dictionary<string, Brush> CustomBrushes = new();
        private static readonly List<Func<IEnumerable<Brush>>> DeferredInitialization = new();

        private delegate Brush LoadBrushFromDelegate(BrushFactory instance, XmlNode brushNode);
        private static readonly LoadBrushFromDelegate? LoadBrushFrom =
            AccessTools2.GetDelegate<LoadBrushFromDelegate>(typeof(BrushFactory), "LoadBrushFrom");

        private static Harmony? _harmony;
        private static WeakReference<BrushFactory?> BrushFactoryReference { get; } = new(null);
        public static void SetBrushFactory(BrushFactory brushFactory)
        {
            BrushFactoryReference.SetTarget(brushFactory);

            foreach (var brush in DeferredInitialization.SelectMany(func => func()))
            {
                CustomBrushes[brush.Name] = brush;
            }
        }

        public static IEnumerable<Brush> Create(XmlDocument xmlDocument)
        {
            if (!BrushFactoryReference.TryGetTarget(out var brushFactory) || brushFactory is null)
                yield break;

            foreach (XmlNode brushNode in xmlDocument.SelectSingleNode("Brushes")!.ChildNodes)
            {
                var brush = LoadBrushFrom?.Invoke(brushFactory, brushNode);
                if (brush is not null)
                {
                    yield return brush;
                }
            }
        }
        public static void Register(Func<IEnumerable<Brush>> func) => DeferredInitialization.Add(func);
        public static void CreateAndRegister(XmlDocument xmlDocument) => Register(() => Create(xmlDocument));

        public static void Clear()
        {
            CustomBrushes.Clear();
            DeferredInitialization.Clear();
            BrushFactoryReference.SetTarget(null);
        }

        internal static bool Enable(Harmony harmony)
        {
            _harmony = harmony;

            harmony.Patch(
                AccessTools2.PropertyGetter(typeof(BrushFactory), "Brushes"),
                postfix: new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(GetBrushesPostfix))));

            harmony.Patch(
                AccessTools2.DeclaredMethod(typeof(BrushFactory), "GetBrush"),
                new HarmonyMethod(AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(GetBrushPrefix))));

            var res3 = harmony.TryPatch(
                AccessTools2.Method(typeof(BrushFactory), "LoadBrushes"),
                AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(LoadBrushesPostfix)));
            if (!res3) return false;

            return true;
        }

        private static void GetBrushesPostfix(ref IEnumerable<Brush> __result)
        {
            __result = __result.Concat(CustomBrushes.Values);
        }

        private static bool GetBrushPrefix(string name, Dictionary<string, Brush> ____brushes, ref Brush __result)
        {
            if (____brushes.ContainsKey(name) || !CustomBrushes.ContainsKey(name))
                return true;

            if (CustomBrushes[name] is { } brush)
            {
                __result = brush;
                return false;
            }

            return true;
        }


        private static void LoadBrushesPostfix(ref BrushFactory __instance)
        {
            SetBrushFactory(__instance);

            _harmony?.Unpatch(
                AccessTools2.Method(typeof(BrushFactory), "LoadBrushes"),
                AccessTools2.DeclaredMethod(typeof(BrushFactoryManager), nameof(LoadBrushesPostfix)));
        }

        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}