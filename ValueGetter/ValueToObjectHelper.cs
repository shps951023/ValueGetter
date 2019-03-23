using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ValueGetter
{
    [Flags]
    internal enum Method
    {
        ToString, ToStringByDynamic, CastObject, CastObjectByDynamic
    }

    internal partial class ValueGetterCache<T, TReturn>
    {
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _ToStringFunctions
            = new ConcurrentDictionary<int, Func<T, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _ToStringByDynamicFunctions
            = new ConcurrentDictionary<int, Func<T, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _CastObjectFunctions
            = new ConcurrentDictionary<int, Func<T, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _CastObjectByDynamicFunctions
            = new ConcurrentDictionary<int, Func<T, TReturn>>();

        internal static Func<T, TReturn> GetOrAddCache(Method call, PropertyInfo propertyInfo)
        {
            //it's too slow 
            //var key =  $"{call}|{propertyInfo.DeclaringType.TypeHandle.Value.ToString()}|{propertyInfo.MetadataToken.ToString()}";
            var key = propertyInfo.MetadataToken;

            if (call == Method.ToString)
            {
                var functions = _ToStringFunctions;
                if (functions.TryGetValue(key, out Func<T, TReturn> func))
                    return func;
                return (functions[key] = GetToStringFunction(propertyInfo));
            }
            else if (call == Method.ToStringByDynamic)
            {
                var functions = _ToStringByDynamicFunctions;
                if (functions.TryGetValue(key, out Func<T, TReturn> func))
                    return func;
                return (functions[key] = GetObjectToStringFunction(propertyInfo));
            }
            else if (call == Method.CastObjectByDynamic)
            {
                var functions = _CastObjectByDynamicFunctions;
                if (functions.TryGetValue(key, out Func<T, TReturn> func))
                    return func;
                return (functions[key] = GetObjectFunction(propertyInfo));
            }

            {
                var functions = _CastObjectFunctions;
                if (functions.TryGetValue(key, out Func<T, TReturn> func))
                    return func;
                return (functions[key] = GetFunction(propertyInfo));
            }
        }

        private static Func<T, TReturn> GetFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var convert = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<T, TReturn>>(convert, instance);
            return lambda.Compile();
        }

        private static Func<T, TReturn> GetObjectFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(typeof(TReturn), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var convert2 = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<T, TReturn>>(convert2, instance);
            return lambda.Compile();
        }
    }

    //GetObjectValue generic with object version, this arrangement can avoid Compiler exception error
    public static partial class ValueGetter
    {
        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(MyClass i) => i.MyProperty1 as object ; </code>
        /// </summary>
        public static Dictionary<string, object> GetObjectValues<T>(this T instance)
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(MyClass i) => i.MyProperty1 as object ; </code>
        /// </summary>
        public static object GetObjectValue<T>(this PropertyInfo propertyInfo, T instance)
            => ValueGetterCache<T, object>.GetOrAddCache(Method.CastObject, propertyInfo)(instance);

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ; </code>
        /// </summary>
        public static Dictionary<string, object> GetObjectValues(this object instance)
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ;</code>
        /// </summary>
        public static object GetObjectValue(this PropertyInfo propertyInfo, object instance)
            => ValueGetterCache<object, object>.GetOrAddCache(Method.CastObjectByDynamic, propertyInfo)(instance);
    }
}

