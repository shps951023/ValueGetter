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

    internal partial class ValueGetterCache<TParam, TReturn>
    {
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> _ToStringFunctions
            = new ConcurrentDictionary<int, Func<TParam, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> _ToStringByDynamicFunctions
            = new ConcurrentDictionary<int, Func<TParam, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> _CastObjectFunctions
            = new ConcurrentDictionary<int, Func<TParam, TReturn>>();
        private static readonly ConcurrentDictionary<int, Func<TParam, TReturn>> _CastObjectByDynamicFunctions
            = new ConcurrentDictionary<int, Func<TParam, TReturn>>();

        internal static Func<TParam, TReturn> GetOrAddCache(Method call, PropertyInfo propertyInfo)
        {
            //it's too slow 
            //var key =  $"{call}|{propertyInfo.DeclaringType.TypeHandle.Value.ToString()}|{propertyInfo.MetadataToken.ToString()}";
            var key = propertyInfo.MetadataToken;

            /*The following Code, though ugly, But is the key for efficiency...*/
            if (call == Method.ToString)
            {
                if (_ToStringFunctions.TryGetValue(key, out Func<TParam, TReturn> func))
                    return func;
                return (_ToStringFunctions[key] = GetToStringFunction(propertyInfo));
            }
            else if (call == Method.ToStringByDynamic)
            {
                if (_ToStringByDynamicFunctions.TryGetValue(key, out Func<TParam, TReturn> func))
                    return func;
                return (_ToStringByDynamicFunctions[key] = GetObjectToStringFunction(propertyInfo));
            }
            else if (call == Method.CastObjectByDynamic)
            {
                if (_CastObjectByDynamicFunctions.TryGetValue(key, out Func<TParam, TReturn> func))
                    return func;
                return (_CastObjectByDynamicFunctions[key] = GetObjectFunction(propertyInfo));
            }

            {
                if (_CastObjectFunctions.TryGetValue(key, out Func<TParam, TReturn> func))
                    return func;
                return (_CastObjectFunctions[key] = GetFunction(propertyInfo));
            }
        }

        private static Func<TParam, TReturn> GetFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var convert = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(convert, instance);
            return lambda.Compile();
        }

        private static Func<TParam, TReturn> GetObjectFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(typeof(TReturn), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var convert2 = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<TParam, TReturn>>(convert2, instance);
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
        {
            if (propertyInfo.DeclaringType == typeof(T))    /* avoid using the upcast variable  get error results */
                return ValueGetterCache<T, object>.GetOrAddCache(Method.CastObject, propertyInfo)(instance);
            else
                return ValueGetterCache<object, object>.GetOrAddCache(Method.CastObjectByDynamic, propertyInfo)(instance);
        }

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

