using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Bannerlord.LauncherEx.Helpers
{
    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Ref/IRef.cs
    /// </summary>
    internal interface IRef : INotifyPropertyChanged
    {
        Type Type { get; }
        object? Value { get; set; }
    }

    /// <summary>
    /// https://github.com/Aragas/Bannerlord.MBOptionScreen/blob/dev/src/MCM/Abstractions/Ref/PropertyRef.cs
    /// </summary>
    internal sealed class PropertyRef : IRef, IEquatable<PropertyRef>
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        public PropertyInfo PropertyInfo { get; }
        public object Instance { get; }

        /// <inheritdoc/>
        public Type Type => PropertyInfo.PropertyType;
        /// <inheritdoc/>
        public object? Value
        {
            get => PropertyInfo.GetValue(Instance);
            set
            {
                if (PropertyInfo.CanWrite)
                {
                    PropertyInfo.SetValue(Instance, value);
                    OnPropertyChanged();
                }
            }
        }

        public PropertyRef(PropertyInfo propInfo, object instance)
        {
            PropertyInfo = propInfo;
            Instance = instance;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public bool Equals(PropertyRef? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return PropertyInfo.Equals(other.PropertyInfo) && Instance.Equals(other.Instance);
        }
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((PropertyRef) obj);
        }
        /// <inheritdoc/>
        public override int GetHashCode() => PropertyInfo.GetHashCode() ^ Instance.GetHashCode();
        public static bool operator ==(PropertyRef? left, PropertyRef? right) => Equals(left, right);
        public static bool operator !=(PropertyRef? left, PropertyRef? right) => !Equals(left, right);
    }

    internal sealed class StorageRef<T> : IRef
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public Type Type { get; }

        private T? _value;
        public object? Value
        {
            get => _value;
            set
            {
                if (value is T val && !Equals(_value, val))
                {
                    _value = val;
                    OnPropertyChanged();
                }
            }
        }

        public StorageRef(T? value)
        {
            _value = value;
            Type = value?.GetType() ?? typeof(T);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    internal class ProxyRef<T> : IRef, IEquatable<ProxyRef<T>>
    {
        /// <inheritdoc/>
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Func<T> _getter;
        private readonly Action<T>? _setter;

        /// <inheritdoc/>
        public Type Type => typeof(T);
        /// <inheritdoc/>
        public object? Value
        {
            get => _getter();
            set
            {
                if (_setter is not null && value is T val)
                {
                    _setter(val);
                    OnPropertyChanged();
                }
            }
        }

        public ProxyRef(Func<T> getter, Action<T>? setter)
        {
            _getter = getter ?? throw new ArgumentNullException(nameof(getter));
            _setter = setter;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <inheritdoc/>
        public bool Equals(ProxyRef<T>? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return _getter.Equals(other._getter) && Equals(_setter, other._setter);
        }
        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProxyRef<T>) obj);
        }
        /// <inheritdoc/>
        public override int GetHashCode()
        {
            var hash = 269;
            hash = (hash * 47) + _getter.GetHashCode();
            if (_setter is not null)
                hash = (hash * 47) + _setter.GetHashCode();
            return hash;
        }
        public static bool operator ==(ProxyRef<T>? left, ProxyRef<T>? right) => Equals(left, right);
        public static bool operator !=(ProxyRef<T>? left, ProxyRef<T>? right) => !Equals(left, right);
    }
}