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
        /// <code>string GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ; </code>
        /// </summary>
        public static Dictionary<string, string> GetToStringValues<T>(this T instance) 
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue<T>(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>string GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ; </code>
        /// </summary>
        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance)
            => instance != null ? ValueGetterCache<T, string>.GetOrAddToStringFuntionCache(propertyInfo)(instance) : null;
    }

    //GetObjectValue generic with object version, this arrangement can avoid Compiler exception error
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
            => instance!=null?ValueGetterCache<T, object>.GetOrAddFunctionCache(propertyInfo)(instance):null;
    }

    internal partial class ValueGetterCache<TParam, TReturn>
    {
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> ToStringFunctions = new ConcurrentDictionary<int, Func<TParam, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> Functions = new ConcurrentDictionary<int, Func<TParam, TReturn>>();
    }

    internal partial class ValueGetterCache<TParam, TReturn>
    {
        internal static Func<TParam, TReturn> GetOrAddFunctionCache(PropertyInfo propertyInfo)
        {
            //it's too slow : var key =  $"{call}|{propertyInfo.DeclaringType.TypeHandle.Value.ToString()}|{propertyInfo.MetadataToken.ToString()}";
            var key = propertyInfo.MetadataToken;
            if (Functions.TryGetValue(key, out Func<TParam, TReturn> func))
                return func;
            return (Functions[key] = GetCastObjectFunction(propertyInfo));
        }

        private static Func<TParam, TReturn> GetFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var convert = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(convert, instance);
            return lambda.Compile();
        }

        private static Func<TParam, TReturn> GetCastObjectFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(typeof(TReturn), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var convert2 = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(convert2, instance);
            return lambda.Compile();
        }
    }

    internal partial class ValueGetterCache<TParam, TReturn>
    {
        internal static Func<TParam, TReturn> GetOrAddToStringFuntionCache(PropertyInfo propertyInfo)
        {
            var key = propertyInfo.MetadataToken;
            if (ToStringFunctions.TryGetValue(key, out Func<TParam, TReturn> func))
                return func;
            return (ToStringFunctions[key] = GetCastObjectAndToStringFunction(propertyInfo));
        }

        private static Func<TParam, TReturn> GetToStringFunction(PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            var toStringMethod = propType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == "ToString").First();

            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var tostring = Expression.Call(property, toStringMethod);
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(tostring, instance);

            return lambda.Compile();
        }

        private static Func<TParam, TReturn> GetCastObjectAndToStringFunction(PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            var toStringMethod = propType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == "ToString").First();

            var instance = Expression.Parameter(typeof(TParam), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var tostring = Expression.Call(property, toStringMethod);
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(tostring, instance);

            return lambda.Compile();
        }
    }

    public static partial class PropertyCacheHelper
    {
        private static readonly Dictionary<RuntimeTypeHandle, IList<PropertyInfo>> TypePropertiesCache = new Dictionary<RuntimeTypeHandle, IList<PropertyInfo>>();

        public static IList<PropertyInfo> GetPropertiesFromCache(this Type type)
        {
            if (TypePropertiesCache.TryGetValue(type.TypeHandle, out IList<PropertyInfo> pis))
                return pis;
            return TypePropertiesCache[type.TypeHandle] = type.GetProperties().ToList();
        }
    }
}

