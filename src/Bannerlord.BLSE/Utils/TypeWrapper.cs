using System;
using System.Globalization;
using System.Reflection;

namespace Bannerlord.BLSE.Utils;

internal class TypeWrapper : Type
{
    public override string Name { get; } = default!;
    public override Guid GUID { get; } = default!;
    public override Module Module { get; } = default!;
    public override Assembly Assembly { get; } = default!;
    public override string FullName { get; } = default!;
    public override string Namespace { get; } = default!;
    public override string AssemblyQualifiedName { get; } = default!;
    public override Type BaseType { get; } = default!;
    public override Type UnderlyingSystemType { get; } = default!;

    public TypeWrapper(string location) => Assembly = new AssemblyWrapper(location);

    public override object[] GetCustomAttributes(bool inherit) => new object[] { };
    public override bool IsDefined(Type attributeType, bool inherit) => false;
    public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => new ConstructorInfo[] { };
    public override Type? GetInterface(string name, bool ignoreCase) => null;
    public override Type[] GetInterfaces() => new Type[] { };
    public override EventInfo? GetEvent(string name, BindingFlags bindingAttr) => null;
    public override EventInfo[] GetEvents(BindingFlags bindingAttr) => new EventInfo[] { };
    public override Type[] GetNestedTypes(BindingFlags bindingAttr) => new Type[] { };
    public override Type? GetNestedType(string name, BindingFlags bindingAttr) => null;
    public override Type? GetElementType() => null;
    protected override bool HasElementTypeImpl() => false;
    protected override PropertyInfo? GetPropertyImpl(string name, BindingFlags bindingAttr, Binder? binder, Type? returnType, Type[]? types, ParameterModifier[]? modifiers) => null;
    public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => new PropertyInfo[] { };
    protected override MethodInfo? GetMethodImpl(string name, BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[]? types, ParameterModifier[]? modifiers) => null;
    public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => new MethodInfo[] { };
    public override FieldInfo? GetField(string name, BindingFlags bindingAttr) => null;
    public override FieldInfo[] GetFields(BindingFlags bindingAttr) => new FieldInfo[] { };
    public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => new MemberInfo[] { };
    protected override TypeAttributes GetAttributeFlagsImpl() => TypeAttributes.NotPublic;
    protected override bool IsArrayImpl() => false;
    protected override bool IsByRefImpl() => false;
    protected override bool IsPointerImpl() => false;
    protected override bool IsPrimitiveImpl() => false;
    protected override bool IsCOMObjectImpl() => false;
    public override object? InvokeMember(string name, BindingFlags invokeAttr, Binder? binder, object? target, object?[]? args, ParameterModifier[]? modifiers, CultureInfo? culture, string[]? namedParameters) => null;
    protected override ConstructorInfo? GetConstructorImpl(BindingFlags bindingAttr, Binder? binder, CallingConventions callConvention, Type[] types, ParameterModifier[]? modifiers) => null;


    public override object[] GetCustomAttributes(Type attributeType, bool inherit) => new object[] { };
}