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
        public static Func<T, TReturn> GetOrAddToStringFunctionCache(PropertyInfo propertyInfo, int key)
        {
            if (_Functions.TryGetValue(key, out Func<T, TReturn> func))
                return func;
            return (_Functions[key] = GetToStringFunction(propertyInfo));
        }

        /// <![CDATA[
        ///     string GetterFunction(MyClass i) => i.MyProperty1.ToString() ;  
        /// ]]>
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
    }

    public static partial class ToStringValueHelper
    {
        public static Dictionary<string, string> GetToStringValues<T>(this T instance) => instance == null
                ? null
                : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetToStringValue(instance));

        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance)
        {
            var key = propertyInfo.MetadataToken;
            return ValueObjectCache<T,String>.GetOrAddToStringFunctionCache(propertyInfo, key)(instance);
        }
    }
}

