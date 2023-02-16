using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TaleWorlds.TwoDimension;

namespace Bannerlord.LauncherEx.ResourceManagers
{
    internal static class SpriteDataManager
    {
        // Replaces the Sprite Sheet mechanism with a single texture
        private sealed class SpriteGenericFromTexture : SpriteGeneric
        {
            private delegate void SetIsLoadedDelegate(SpriteCategory instance, bool value);
            private static readonly SetIsLoadedDelegate? SetIsLoaded =
                AccessTools2.GetPropertySetterDelegate<SetIsLoadedDelegate>(typeof(SpriteCategory), "IsLoaded");

            private static SpritePart GetSpritePart(string name, Texture texture)
            {
                var data = new SpriteData(name);
                var category = new SpriteCategory(name, data, 0)
                {
                    SpriteSheets =
                    {
                        texture
                    },
                    SpriteSheetCount = 1
                };
                SetIsLoaded?.Invoke(category, true);

                return new SpritePart(name, category, texture.Width, texture.Height)
                {
                    SheetID = 1,
                };
            }
            public SpriteGenericFromTexture(string name, Texture texture) : base(name, GetSpritePart(name, texture)) { }
        }

        private sealed class SpriteFromTexture : Sprite
        {
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float>? FieldMapX = AccessTools2.StructFieldRefAccess<SpriteDrawData, float>("MapX");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float>? FieldMapY = AccessTools2.StructFieldRefAccess<SpriteDrawData, float>("MapY");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float>? FieldScale = AccessTools2.StructFieldRefAccess<SpriteDrawData, float>("Scale");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float>? FieldWidth = AccessTools2.StructFieldRefAccess<SpriteDrawData, float>("Width");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, float>? FieldHeight = AccessTools2.StructFieldRefAccess<SpriteDrawData, float>("Height");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, bool>? FieldHorizontalFlip = AccessTools2.StructFieldRefAccess<SpriteDrawData, bool>("HorizontalFlip");
            private static readonly AccessTools.StructFieldRef<SpriteDrawData, bool>? FieldVerticalFlip = AccessTools2.StructFieldRefAccess<SpriteDrawData, bool>("VerticalFlip");

            private static readonly Type? Vector2 = Type.GetType("System.Numerics.Vector2, System.Numerics.Vectors");
            private static readonly ConstructorInfo? Vector2Constructor = AccessTools2.Constructor(Vector2!, new[] { typeof(float), typeof(float) });
            private static readonly MethodInfo? CreateQuad = AccessTools2.Method("TaleWorlds.TwoDimension.DrawObject2D:CreateQuad");


            public override Texture Texture { get; }

            private readonly float[] _vertices;
            private readonly float[] _uvs;
            private readonly uint[] _indices;

            public SpriteFromTexture(Texture texture) : this("Sprite", texture) { }
            public SpriteFromTexture(string name, Texture texture) : base(name, texture.Width, texture.Height)
            {
                Texture = texture;
                _vertices = new float[8];
                _uvs = new float[8];
                _indices = new uint[6];
                _indices[0] = 0U;
                _indices[1] = 1U;
                _indices[2] = 2U;
                _indices[3] = 0U;
                _indices[4] = 2U;
                _indices[5] = 3U;
            }

            public override float GetScaleToUse(float width, float height, float scale) => scale;

            protected override DrawObject2D GetArrays(SpriteDrawData spriteDrawData)
            {
                if (CachedDrawObject is not null && CachedDrawData == spriteDrawData)
                    return CachedDrawObject;

                if (FieldMapX is null || FieldMapY is null || FieldWidth is null || FieldHeight is null || FieldHorizontalFlip is null || FieldVerticalFlip is null)
                    return null!;

                var mapX = FieldMapX(ref spriteDrawData);
                var mapY = FieldMapY(ref spriteDrawData);
                var width = FieldWidth(ref spriteDrawData);
                var height = FieldHeight(ref spriteDrawData);
                var horizontalFlip = FieldHorizontalFlip(ref spriteDrawData);
                var verticalFlip = FieldVerticalFlip(ref spriteDrawData);

                //var vec2 = Vector2Constructor.Invoke(new object[] { width, height });
                //var quad1 = CreateQuad.Invoke(null, new object[]{ vec2 }) as DrawObject2D;
                //return quad1;
                //var quad = DrawObject2D.CreateQuad(new Vector2(width, height));
                //return quad;

                if (mapX == 0f && mapY == 0f)
                {
                    PopulateVertices(Texture, mapX, mapY, _vertices, 0, 1f, width, height);
                    PopulateTextureCoordinates(_uvs, 0, horizontalFlip, verticalFlip);
                    var drawObject2D = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6)
                    {
                        DrawObjectType = DrawObjectType.Quad,
                        Width = width,
                        Height = height,
                        MinU = 0f,
                        MaxU = 1f,
                        MinV = 0f,
                        MaxV = 1f
                    };
                    if (horizontalFlip)
                    {
                        drawObject2D.MinU = 1f;
                        drawObject2D.MaxU = 0f;
                    }
                    if (verticalFlip)
                    {
                        drawObject2D.MinV = 1f;
                        drawObject2D.MaxV = 0f;
                    }

                    CachedDrawData = spriteDrawData;
                    CachedDrawObject = drawObject2D;
                    return drawObject2D;
                }

                PopulateVertices(Texture, mapX, mapY, _vertices, 0, 1f, width, height);
                PopulateTextureCoordinates(_uvs, 0, horizontalFlip, verticalFlip);
                var drawObject2D2 = new DrawObject2D(MeshTopology.Triangles, _vertices.ToArray(), _uvs.ToArray(), _indices.ToArray(), 6)
                {
                    DrawObjectType = DrawObjectType.Mesh
                };

                CachedDrawData = spriteDrawData;
                CachedDrawObject = drawObject2D2;
                return drawObject2D2;
            }

            private static void PopulateVertices(Texture texture, float screenX, float screenY, float[] outVertices, int verticesStartIndex, float scale, float customWidth, float customHeight)
            {
                var widthProp = customWidth / texture.Width;
                var heightProp = customHeight / texture.Height;
                var widthScaled = texture.Width * scale * widthProp;
                var heightScaled = texture.Height * scale * heightProp;

                outVertices[verticesStartIndex] = screenX + 0f;
                outVertices[verticesStartIndex + 1] = screenY + 0f;
                outVertices[verticesStartIndex + 2] = screenX + 0f;
                outVertices[verticesStartIndex + 3] = screenY + heightScaled;
                outVertices[verticesStartIndex + 4] = screenX + widthScaled;
                outVertices[verticesStartIndex + 5] = screenY + heightScaled;
                outVertices[verticesStartIndex + 6] = screenX + widthScaled;
                outVertices[verticesStartIndex + 7] = screenY + 0f;
            }
            private static void PopulateTextureCoordinates(float[] outUVs, int uvsStartIndex, bool horizontalFlip, bool verticalFlip)
            {
                var minU = 0f;
                var maxU = 1f;
                if (horizontalFlip)
                {
                    minU = 1f;
                    maxU = 0f;
                }

                var minV = 0f;
                var maxV = 1f;
                if (verticalFlip)
                {
                    minV = 1f;
                    maxV = 0f;
                }

                outUVs[uvsStartIndex] = minU;
                outUVs[uvsStartIndex + 1] = minV;
                outUVs[uvsStartIndex + 2] = minU;
                outUVs[uvsStartIndex + 3] = maxV;
                outUVs[uvsStartIndex + 4] = maxU;
                outUVs[uvsStartIndex + 5] = maxV;
                outUVs[uvsStartIndex + 6] = maxU;
                outUVs[uvsStartIndex + 7] = minV;
            }
        }


        private static readonly Dictionary<string, Sprite> SpriteNames = new();
        private static readonly List<Func<Sprite>> DeferredInitialization = new();

        public static Sprite? Create(string name) => GraphicsContextManager.Instance.TryGetTarget(out var gc) && gc is not null
            ? new SpriteFromTexture(name, new Texture(gc.GetTexture(name))) : null;
        public static Sprite? CreateGeneric(string name) => GraphicsContextManager.Instance.TryGetTarget(out var gc) && gc is not null
            ? new SpriteGenericFromTexture(name, new Texture(gc.GetTexture(name))) : null;
        public static void Register(Func<Sprite> func) => DeferredInitialization.Add(func);
        public static void CreateAndRegister(string name) => Register(() => Create(name)!);
        public static void CreateGenericAndRegister(string name) => Register(() => CreateGeneric(name)!);

        public static void Clear()
        {
            SpriteNames.Clear();
            DeferredInitialization.Clear();
        }

        internal static bool Enable(Harmony harmony)
        {
            var res1 = harmony.TryPatch(
                AccessTools2.Method(typeof(SpriteData), "GetSprite"),
                prefix: AccessTools2.DeclaredMethod(typeof(SpriteDataManager), nameof(GetSpritePrefix)));
            if (!res1) return false;

            var res2 = harmony.TryPatch(
                AccessTools2.Method(typeof(SpriteData), "Load"),
                postfix: AccessTools2.DeclaredMethod(typeof(SpriteDataManager), nameof(LoadPostfix)));
            if (!res2) return false;

            return true;
        }

        private static bool GetSpritePrefix(string name, ref Sprite __result)
        {
            if (!SpriteNames.TryGetValue(name, out __result))
                return true;
            return false;
        }

        private static void LoadPostfix()
        {
            foreach (var func in DeferredInitialization)
            {
                var sprite = func();
                SpriteNames[sprite.Name] = sprite;
            }
        }

        private static IEnumerable<CodeInstruction> BlankTranspiler(IEnumerable<CodeInstruction> instructions) => instructions;
    }
}