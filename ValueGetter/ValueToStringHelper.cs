using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ValueGetter
{
    internal partial class ValueGetterCache<TParam, TReturn>
    {

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


        private static Func<TParam, TReturn> GetObjectToStringFunction(PropertyInfo prop)
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

    public static partial class ValueGetter
    {
        /// <summary>
        /// Compiler Method Like:
        /// <code>string GetterFunction(MyClass i) => i.MyProperty1.ToString() ; </code>
        /// </summary>
        public static Dictionary<string, string> GetToStringValues<T>(this T instance) 
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>string GetterFunction(MyClass i) => i.MyProperty1.ToString() ; </code>
        /// </summary>
        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance)
        {
            if (propertyInfo.DeclaringType == typeof(T))    /* avoid using the upcast variable  get error results */
                return ValueGetterCache<T, String>.GetOrAddCache(Method.ToString, propertyInfo)(instance);
            else
                return ValueGetterCache<object, String>.GetOrAddCache(Method.ToStringByDynamic, propertyInfo)(instance);
        }

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ;</code> 
        /// </summary>
        public static Dictionary<string, string> GetToStringValues(this object instance) 
            => instance?.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue(instance));

        /// <summary>
        /// Compiler Method Like:
        /// <code>object GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ; </code>
        /// </summary>
        public static string GetToStringValue(this PropertyInfo propertyInfo, object instance) 
            => ValueGetterCache<object, String>.GetOrAddCache(Method.ToStringByDynamic, propertyInfo)(instance);
    }
}

