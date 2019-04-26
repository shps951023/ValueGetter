using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ValueGetter
{
    public static partial class ValueGetter
    {
        /// <summary>
        /// Compiler Method Like:
        /// <code>
        ///     string GetterToStringFunction(object i) => GetterFunction(i).ToString() ; 
        ///     object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ;
        /// </code>
        /// </summary>
        public static Dictionary<string, string> GetToStringValues<T>(this T instance)
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue<T>(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>
        ///     string GetterToStringFunction(object i) => GetterFunction(i).ToString() ; 
        ///     object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ;
        /// </code>
        /// </summary>
        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance)
            => instance != null ? ValueGetterCache<T, object>.GetOrAddFunctionCache(propertyInfo)(instance)?.ToString() : null;
    }

    public static partial class ValueGetter
    {
        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ; </code>
        /// </summary>
        public static Dictionary<string, object> GetObjectValues<T>(this T instance)
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ; </code>
        /// </summary>
        public static object GetObjectValue<T>(this PropertyInfo propertyInfo, T instance)
            => instance != null ? ValueGetterCache<T, object>.GetOrAddFunctionCache(propertyInfo)(instance) : null;
    }

    internal partial class ValueGetterCache<TParam, TReturn>
    {
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> Functions = new ConcurrentDictionary<int, Func<TParam, TReturn>>();

        internal static Func<TParam, TReturn> GetOrAddFunctionCache(PropertyInfo propertyInfo)
        {
            var key = propertyInfo.MetadataToken;
            if (Functions.TryGetValue(key, out Func<TParam, TReturn> func))
                return func;
            return (Functions[key] = GetCastObjectFunction(propertyInfo));
        }

        private static Func<TParam, TReturn> GetCastObjectFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(typeof(TReturn), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var cast = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(cast, instance);
            return lambda.Compile();
        }
    }

    public static partial class PropertyCacheHelper
    {
        private static readonly Dictionary<RuntimeTypeHandle, IList<PropertyInfo>> TypePropertiesCache = new Dictionary<RuntimeTypeHandle, IList<PropertyInfo>>();
        private static readonly Dictionary<RuntimeTypeHandle, IDictionary<string, PropertyInfo>> TypePropertiesDictionaryCache = new Dictionary<RuntimeTypeHandle, IDictionary<string, PropertyInfo>>();

        public static IList<PropertyInfo> GetPropertiesFromCache(this Type type)
        {
            if (TypePropertiesCache.TryGetValue(type.TypeHandle, out IList<PropertyInfo> pis))
                return pis;
            return TypePropertiesCache[type.TypeHandle] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(w => w.CanRead).ToList();
        }

        public static IList<PropertyInfo> GetPropertiesFromCache(this object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return instance.GetType().GetPropertiesFromCache();
        }

        public static IDictionary<string, PropertyInfo> GetPropertiesDictionaryFromCache(this Type type)
        {
            if (TypePropertiesDictionaryCache.TryGetValue(type.TypeHandle, out IDictionary<string, PropertyInfo> pis))
                return pis;
            return TypePropertiesDictionaryCache[type.TypeHandle] = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(w => w.CanRead)
                .ToDictionary(d => d.Name, d => d);
        }

        public static IDictionary<string, PropertyInfo> GetPropertiesDictionaryFromCache(this object instance)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            return instance.GetType().GetPropertiesDictionaryFromCache();
        }
    }
}

