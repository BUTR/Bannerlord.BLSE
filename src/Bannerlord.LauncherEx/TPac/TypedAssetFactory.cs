using System;
using System.Collections.Generic;
using System.Reflection;

namespace Bannerlord.LauncherEx.TPac;

internal static class TypedAssetFactory
{
    private static readonly Dictionary<Guid, Type> classMap = new();
    private static readonly Dictionary<Guid, ConstructorInfo> constructorMap = new();

    static TypedAssetFactory()
    {
        RegisterType(typeof(Texture));
    }

    public static void RegisterType(Type typeClass)
    {
        var field = typeClass.GetField("TYPE_GUID", BindingFlags.Static | BindingFlags.Public) ?? throw new ArgumentException("Cannot find public static field \"TYPE_GUID\" from class " + typeClass.FullName);
        if (field.FieldType != typeof(Guid)) throw new ArgumentException("\"TYPE_GUID\" must be Guid");
        var guid = (Guid) field.GetValue(null);
        RegisterType(guid, typeClass);
    }

    public static void RegisterType(Guid typeGuid, Type typeClass)
    {
        if (!typeof(AssetItem).IsAssignableFrom(typeClass))
            throw new ArgumentException("Registered type must extend from AssetItem");

        var constructor = typeClass.GetConstructor(Type.EmptyTypes);
        if (constructor == null)
            throw new ArgumentException("Registered type must have a param-less constructor");

        classMap[typeGuid] = typeClass;
        constructorMap[typeGuid] = constructor;
    }

    public static bool CreateTypedAsset(Guid typeGuid, out AssetItem result)
    {
        if (classMap.ContainsKey(typeGuid))
        {
            var constructor = constructorMap[typeGuid];
            result = (AssetItem) constructor.Invoke(null);
            return true;
        }

        result = new AssetItem(typeGuid);
        return false;
    }
}