using System;
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
}

