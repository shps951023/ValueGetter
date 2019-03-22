using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ValueGetter
{

    internal enum Method
    {
        ToString, ToStringByDynamic, CastObject, CastObjectByDynamic
    }

    public partial class ValueObjectCache<T, TReturn>
    {
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _Functions = new ConcurrentDictionary<int, Func<T, TReturn>>();
        internal static Func<T, TReturn> GetOrAddCache(Method call, PropertyInfo propertyInfo)
        {
            var key = propertyInfo.MetadataToken;
            if (_Functions.TryGetValue(key, out Func<T, TReturn> func))
                return func;

            if (call == Method.ToString)
                return (_Functions[key] = GetToStringFunction(propertyInfo));
            if (call == Method.ToStringByDynamic)
                return (_Functions[key] = GetDynamicToStringFunction(propertyInfo));
            if (call == Method.CastObjectByDynamic)
                return (_Functions[key] = GetDynamicFunction(propertyInfo));
            return (_Functions[key] = GetFunction(propertyInfo));
        }

        private static Func<T, TReturn> GetFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var convert = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<T, TReturn>>(convert, instance);
            return lambda.Compile();
        }

        private static Func<T, TReturn> GetDynamicFunction(PropertyInfo prop)
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
    public static partial class ValueObjectHelper
    {
        /// <summary>
        /// compiler like : object GetterFunction(MyClass i) => i.MyProperty1 as object ; 
        /// </summary>
        public static Dictionary<string, object> GetObjectValues<T>(this T instance) => instance == null
                ? null
                : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        /// <summary>
        /// compiler like : object GetterFunction(MyClass i) => i.MyProperty1 as object ; 
        /// </summary>
        public static object GetObjectValue<T>(this PropertyInfo propertyInfo, T instance) => ValueObjectCache<T, object>.GetOrAddCache(Method.CastObject, propertyInfo)(instance);

        /// <summary>
        /// compiler like : object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ; 
        /// </summary>
        public static Dictionary<string, object> GetObjectValues(this object instance) => instance == null 
            ? null 
            : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        /// <summary>
        /// compiler like : object GetterFunction(object i) => (i as MyClass).MyProperty1 as object ;
        /// </summary>
        public static object GetObjectValue(this PropertyInfo propertyInfo, object instance) => ValueObjectCache<object, object>.GetOrAddCache(Method.CastObjectByDynamic, propertyInfo)(instance);
    }
}

