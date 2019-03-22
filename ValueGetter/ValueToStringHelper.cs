using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace ValueGetter
{
    public partial class ValueObjectCache<T, TReturn>
    {

        private static Func<T, TReturn> GetToStringFunction(PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            var toStringMethod = propType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == "ToString").First();

            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var tostring = Expression.Call(property, toStringMethod);

            var lambda = Expression.Lambda<Func<T, TReturn>>(tostring, instance);
            return lambda.Compile();
        }


        private static Func<T, TReturn> GetDynamicToStringFunction(PropertyInfo prop)
        {
            var propType = prop.PropertyType;
            var toStringMethod = propType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == "ToString").First();

            var instance = Expression.Parameter(typeof(T), "i");
            var convert = Expression.TypeAs(instance, prop.DeclaringType);
            var property = Expression.Property(convert, prop);
            var tostring = Expression.Call(property, toStringMethod);
            var lambda = Expression.Lambda<Func<T, TReturn>>(tostring, instance);

            return lambda.Compile();
        }
    }

    public static partial class ToStringValueHelper
    {
        /// <summary>
        /// compiler like : string GetterFunction(MyClass i) => i.MyProperty1.ToString() ; 
        /// </summary>
        public static Dictionary<string, string> GetToStringValues<T>(this T instance) => instance == null
                ? null
                : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue(instance));

        /// <summary>
        /// compiler like : string GetterFunction(MyClass i) => i.MyProperty1.ToString() ; 
        /// </summary>
        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance) => ValueObjectCache<T, String>.GetOrAddCache(Method.ToString,propertyInfo)(instance);

        /// <summary>
        /// compiler like : object GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ; 
        /// </summary>
        public static Dictionary<string, string> GetToStringValues(this object instance) => instance == null
                ? null
                : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue(instance));

        /// <summary>
        /// compiler like : object GetterFunction(object i) => (i as MyClass).MyProperty1.ToString() ; 
        /// </summary>
        public static string GetToStringValue(this PropertyInfo propertyInfo, object instance) => ValueObjectCache<object, String>.GetOrAddCache(Method.ToStringByDynamic, propertyInfo)(instance);
    }
}

