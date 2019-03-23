using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ValueGetter
{
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
}

