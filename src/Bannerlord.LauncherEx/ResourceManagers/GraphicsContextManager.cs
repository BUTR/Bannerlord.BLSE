using Bannerlord.BUTR.Shared.Extensions;
using Bannerlord.LauncherEx.Extensions;

using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.IO;

using TaleWorlds.TwoDimension.Standalone;

namespace Bannerlord.LauncherEx.ResourceManagers
{
    internal static class GraphicsContextManager
    {
        public static WeakReference<GraphicsContext?> Instance { get; private set; } = default!;

        private static readonly Dictionary<string, OpenGLTexture> Textures = new();
        private static readonly Dictionary<string, Func<OpenGLTexture>> DeferredInitialization = new();

        public static OpenGLTexture Create(string name, Stream stream)
        {
            var texture = new OpenGLTexture();
            if (texture.LoadFromStream(name, stream))
                return texture;

            var path = Path.GetTempFileName();
            using (var fs = File.OpenWrite(path))
                stream.CopyTo(fs);
            var openGLTexture = OpenGLTexture.FromFile(path);
            File.Delete(path);

            return openGLTexture;
        }
        private static OpenGLTexture CreateAssetTexture(string name, TPac.Texture assetTexture)
        {
            var texture = new OpenGLTexture();
            texture.LoadFromAssetTexture(name, assetTexture);
            return texture;
        }
        public static void Register(string name, Func<OpenGLTexture> func) => DeferredInitialization.Add(name, func);
        public static void CreateAndRegister(string name, Stream stream) => Register(name, () => Create(name, stream));
        public static void CreateAssetTextureAndRegister(string name, TPac.Texture texture) => Register(name, () => CreateAssetTexture(name, texture));

        public static void Clear()
        {
            Textures.Clear();
            DeferredInitialization.Clear();
            Instance.SetTarget(null);
        }

        internal static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(GraphicsContext), "GetTexture"),
                prefix: AccessTools2.DeclaredMethod(typeof(GraphicsContextManager), nameof(GetTexturePrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.DeclaredMethod(typeof(GraphicsContext), "CreateContext"),
                postfix: AccessTools2.DeclaredMethod(typeof(GraphicsContextManager), nameof(CreateContextPostfix)));
            if (!res2) return false;

            return true;
        }

        private static bool GetTexturePrefix(string textureName, ref OpenGLTexture __result)
        {
            if (!Textures.TryGetValue(textureName, out __result))
                return true;
            return false;
        }

        private static void CreateContextPostfix(GraphicsContext __instance)
        {
            Instance = new(__instance);

            foreach (var (name, func) in DeferredInitialization)
            {
                Textures[name] = func();
            }
        }

        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}