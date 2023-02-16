using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Bannerlord.LauncherEx.TPac
{
    internal static class TypedDataFactory
    {
        private static readonly Dictionary<Guid, Type> guidToLoaderTypeMap = new();
        private static readonly Dictionary<Guid, ConstructorInfo> guidToLoaderConstructorMap = new();

        static TypedDataFactory()
        {
            RegisterType(typeof(TexturePixelData));
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
            if (!typeof(ExternalData).IsAssignableFrom(typeClass))
                throw new ArgumentException("Registered type must extend from ExternalData");

            var constructor = typeClass.GetConstructor(Type.EmptyTypes);
            if (constructor == null) throw new ArgumentException("Registered type must have a param-less constructor");

            var loaderType = typeof(ExternalLoader<>).MakeGenericType(typeClass);
            guidToLoaderTypeMap[typeGuid] = loaderType;
            guidToLoaderConstructorMap[typeGuid] = loaderType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(FileInfo) }, null);
        }

        public static bool CreateTypedLoader(Guid typeGuid, FileInfo file, out AbstractExternalLoader result)
        {
            AbstractExternalLoader? loader = null;
            var isFound = false;
            if (guidToLoaderTypeMap.ContainsKey(typeGuid))
            {
                var constructor = guidToLoaderConstructorMap[typeGuid];
                loader = (AbstractExternalLoader) constructor.Invoke(new object[] { file });
                isFound = true;
            }

            loader ??= new ExternalLoader<ExternalData>(file);

            loader.TypeGuid = typeGuid;
            result = loader;
            return isFound;
        }
    }
}