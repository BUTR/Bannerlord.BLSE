using HarmonyLib;
using HarmonyLib.BUTR.Extensions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

using TaleWorlds.GauntletUI;
using TaleWorlds.GauntletUI.BaseTypes;
using TaleWorlds.Library;

namespace Bannerlord.LauncherEx.Extensions
{
    internal static class WidgetExtensions
    {
        private delegate void OnPropertyChangedDelegate1<T>(PropertyOwnerObject instance, T value, [CallerMemberName] string? propertyName = null);
        private static readonly ConcurrentDictionary<Type, Delegate> OnPropertyChanged1 =
            new();

        private delegate void OnPropertyChangedDelegate2(PropertyOwnerObject instance, bool value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate2? OnPropertyChanged2 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate2>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(bool), typeof(string) });

        private delegate void OnPropertyChangedDelegate3(PropertyOwnerObject instance, int value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate3? OnPropertyChanged3 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate3>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(int), typeof(string) });

        private delegate void OnPropertyChangedDelegate4(PropertyOwnerObject instance, float value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate4? OnPropertyChanged4 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate4>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(float), typeof(string) });

        private delegate void OnPropertyChangedDelegate5(PropertyOwnerObject instance, uint value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate5? OnPropertyChanged5 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate5>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(uint), typeof(string) });

        private delegate void OnPropertyChangedDelegate6(PropertyOwnerObject instance, Color value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate6? OnPropertyChanged6 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate6>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(Color), typeof(string) });

        private delegate void OnPropertyChangedDelegate7(PropertyOwnerObject instance, double value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate7? OnPropertyChanged7 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate7>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(double), typeof(string) });

        private delegate void OnPropertyChangedDelegate8(PropertyOwnerObject instance, Vec2 value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate8? OnPropertyChanged8 =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate8>(typeof(PropertyOwnerObject), "OnPropertyChanged", new[] { typeof(Vec2), typeof(string) });


        private delegate void OnPropertyChangedDelegate<in T>(PropertyOwnerObject instance, T value, [CallerMemberName] string? propertyName = null);
        private static readonly OnPropertyChangedDelegate<string>? OnPropertyChangedMethod =
            AccessTools2.GetDelegate<OnPropertyChangedDelegate<string>>(typeof(PropertyOwnerObject), "OnPropertyChanged");

        private delegate void SetIsPressedDelegate(Widget instance, bool value);
        private static readonly SetIsPressedDelegate? SetIsPressedMethod =
            AccessTools2.GetPropertySetterDelegate<SetIsPressedDelegate>(typeof(Widget), "IsPressed");

        public static bool IsPointInsideMeasuredArea(this Widget widget)
        {
            var method = AccessTools2.Method(typeof(Widget), "IsPointInsideMeasuredArea");
            var property = AccessTools2.Property(typeof(EventManager), "MousePosition");

            if (method is null || property is null)
                return false;

            if (method.Invoke(widget, new[] { property.GetValue(widget.EventManager) }) is not bool result)
                return false;

            return result;
        }

        public static void SetIsPressed(this Widget widget, bool value) => SetIsPressedMethod?.Invoke(widget, value);

        public static bool SetField<T>(this Widget widget, ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }
            field = value;

            switch (value)
            {
                case bool val when OnPropertyChanged2 is not null: OnPropertyChanged2(widget, val, propertyName); return true;
                case int val when OnPropertyChanged3 is not null: OnPropertyChanged3(widget, val, propertyName); return true;
                case float val when OnPropertyChanged4 is not null: OnPropertyChanged4(widget, val, propertyName); return true;
                case uint val when OnPropertyChanged5 is not null: OnPropertyChanged5(widget, val, propertyName); return true;
                case Color val when OnPropertyChanged6 is not null: OnPropertyChanged6(widget, val, propertyName); return true;
                case double val when OnPropertyChanged7 is not null: OnPropertyChanged7(widget, val, propertyName); return true;
                case Vec2 val when OnPropertyChanged8 is not null: OnPropertyChanged8(widget, val, propertyName); return true;
            }

            static Delegate ValueFactory(Type _)
            {
                var method = AccessTools.GetDeclaredMethods(typeof(PropertyOwnerObject)).FirstOrDefault(x => x.IsGenericMethod && x.Name == "OnPropertyChanged")?.MakeGenericMethod(typeof(T))!;
                return AccessTools2.GetDelegate<OnPropertyChangedDelegate1<T>>(method)!;
            }

            if (OnPropertyChanged1.GetOrAdd(typeof(T), ValueFactory) is OnPropertyChangedDelegate1<T> del)
                del(widget, value, propertyName);
            return true;
        }
    }
}