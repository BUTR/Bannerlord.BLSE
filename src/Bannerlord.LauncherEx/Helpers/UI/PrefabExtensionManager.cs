using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;

namespace Bannerlord.LauncherEx.Helpers;

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Prefabs/IPrefabPatch.cs
/// </summary>
internal interface IPrefabPatch { }

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Prefabs/PrefabExtensionSetAttributePatch.cs
/// </summary>
internal abstract class PrefabExtensionSetAttributePatch : IPrefabPatch
{
    public abstract string Attribute { get; }
    public abstract string Value { get; }
}
internal abstract class PrefabExtensionSetAttributesPatch : IPrefabPatch
{
    public abstract List<Attribute> Attributes { get; }

    public record struct Attribute(string Name, string Value);
}

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Prefabs/PrefabExtensionInsertAsSiblingPatch.cs
/// </summary>
internal abstract class PrefabExtensionInsertAsSiblingPatch : IPrefabPatch
{
    public enum InsertType { Prepend, Append }

    public virtual InsertType Type => InsertType.Append;

    public abstract XmlDocument GetPrefabExtension();
}

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Prefabs/PrefabExtensionReplacePatch.cs
/// </summary>
internal abstract class PrefabExtensionReplacePatch : IPrefabPatch
{
    public abstract XmlDocument GetPrefabExtension();
}

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Prefabs/CustomPatch.cs
/// </summary>
internal abstract class PrefabExtensionCustomPatch<T> : IPrefabPatch where T : XmlNode
{
    public abstract void Apply(T obj);
}

/// <summary>
/// https://github.com/BUTR/Bannerlord.UIExtenderEx/blob/dev/src/Bannerlord.UIExtenderEx/Components/PrefabComponent.cs
/// </summary>
internal static class PrefabExtensionManager
{
    private static readonly ConcurrentDictionary<string, List<Action<XmlDocument>>> MoviePatches = new();

    public static void RegisterPatch(string movie, Action<XmlDocument> patcher)
    {
        if (string.IsNullOrEmpty(movie))
        {
            return;
        }

        MoviePatches.GetOrAdd(movie, _ => []).Add(patcher);
    }

    public static void RegisterPatch(string movie, string? xpath, Action<XmlNode> patcher)
    {
        RegisterPatch(movie, document =>
        {
            var node = document.SelectSingleNode(xpath ?? string.Empty);
            if (node is null)
            {
                return;
            }

            patcher(node);
        });
    }

    public static void RegisterPatch(string movie, string? xpath, PrefabExtensionCustomPatch<XmlNode> patcher)
    {
        RegisterPatch(movie, document =>
        {
            var node = document.SelectSingleNode(xpath ?? string.Empty);
            if (node is null)
            {
                return;
            }

            patcher.Apply(node);
        });
    }

    public static void RegisterPatch(string movie, string? xpath, PrefabExtensionSetAttributePatch patch)
    {
        RegisterPatch(movie, xpath, node =>
        {
            var ownerDocument = node as XmlDocument ?? node.OwnerDocument;
            if (ownerDocument is null)
            {
                return;
            }

            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            if (node.Attributes![patch.Attribute] is null)
            {
                var attribute = ownerDocument.CreateAttribute(patch.Attribute);
                node.Attributes.Append(attribute);
            }

            node.Attributes![patch.Attribute].Value = patch.Value;
        });
    }

    public static void RegisterPatch(string movie, string? xpath, PrefabExtensionSetAttributesPatch patch)
    {
        RegisterPatch(movie, xpath, node =>
        {
            var ownerDocument = node as XmlDocument ?? node.OwnerDocument;
            if (ownerDocument is null)
            {
                return;
            }

            if (node.NodeType != XmlNodeType.Element)
            {
                return;
            }

            foreach (var (attribute, value) in patch.Attributes)
            {
                if (node.Attributes![attribute] is null)
                {
                    var attr = ownerDocument.CreateAttribute(attribute);
                    node.Attributes.Append(attr);
                }

                node.Attributes![attribute].Value = value;
            }
        });
    }

    public static void RegisterPatch(string movie, string? xpath, PrefabExtensionReplacePatch patch)
    {
        RegisterPatch(movie, xpath, node =>
        {
            var ownerDocument = node as XmlDocument ?? node.OwnerDocument;
            if (ownerDocument is null)
            {
                return;
            }

            if (node.ParentNode is null)
            {
                return;
            }

            var extensionNode = patch.GetPrefabExtension().DocumentElement;
            if (extensionNode is null)
            {
                return;
            }

            var importedExtensionNode = ownerDocument.ImportNode(extensionNode, true);

            node.ParentNode.ReplaceChild(importedExtensionNode, node);
        });
    }

    public static void RegisterPatch(string movie, string? xpath, PrefabExtensionInsertAsSiblingPatch patch)
    {
        RegisterPatch(movie, xpath, node =>
        {
            var ownerDocument = node as XmlDocument ?? node.OwnerDocument;
            if (ownerDocument is null)
            {
                return;
            }

            if (node.ParentNode is null)
            {
                return;
            }

            var extensionNode = patch.GetPrefabExtension().DocumentElement;
            if (extensionNode is null)
            {
                return;
            }

            var importedExtensionNode = ownerDocument.ImportNode(extensionNode, true);

            switch (patch.Type)
            {
                case PrefabExtensionInsertAsSiblingPatch.InsertType.Append:
                    node.ParentNode.InsertAfter(importedExtensionNode, node);
                    break;

                case PrefabExtensionInsertAsSiblingPatch.InsertType.Prepend:
                    node.ParentNode.InsertBefore(importedExtensionNode, node);
                    break;
            }
        });
    }


    public static void ProcessMovieIfNeeded(string movie, XmlDocument document)
    {
        if (!MoviePatches.TryGetValue(movie, out var patches))
            return;

        foreach (var patch in patches)
        {
            patch(document);
        }
    }
}