using System;
using System.Globalization;
using System.Reflection;

namespace Bannerlord.LauncherEx.Helpers;

internal class TypeWrapper : Type
{
    public override string Name { get; } = null!;
    public override Guid GUID { get; } = Guid.Empty;
    public override Module Module { get; } = null!;
    public override Assembly Assembly { get; } = null!;
    public override string FullName { get; } = null!;
    public override string Namespace { get; } = null!;
    public override string AssemblyQualifiedName { get; } = null!;
    public override Type BaseType { get; } = null!;
    public override Type UnderlyingSystemType { get; } = null!;

    public TypeWrapper(string location) => Assembly = new AssemblyWrapper(location);

    public override object[] GetCustomAttributes(bool inherit) => [];
    public override bool IsDefined(Type attributeType, bool inherit) => false;
    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => [];
    public override Type? GetInterface(string name, bool ignoreCase) => null;
    public override Type[] GetInterfaces() => [];
    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => null;
    public override EventInfo[] GetEvents(BindingFlags bindingAttr) => [];
    public override Type[] GetNestedTypes(BindingFlags bindingAttr) => [];
    public override Type? GetNestedType(string name, BindingFlags bindingAttr) => null;
    public override Type? GetElementType() => null;
    protected override bool HasElementTypeImpl() => false;
    protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers) => null;
    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => [];
    protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => null;
    public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => [];
    public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => null;
    public override FieldInfo[] GetFields(BindingFlags bindingAttr) => [];
    public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => [];
    protected override TypeAttributes GetAttributeFlagsImpl() => TypeAttributes.NotPublic;
    protected override bool IsArrayImpl() => false;
    protected override bool IsByRefImpl() => false;
    protected override bool IsPointerImpl() => false;
    protected override bool IsPrimitiveImpl() => false;
    protected override bool IsCOMObjectImpl() => false;
    public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters) => null;
    protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers) => null;


    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => [];
}