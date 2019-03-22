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
        private static readonly ConcurrentDictionary<int, Func<T, TReturn>> _Functions = new ConcurrentDictionary<int, Func<T, TReturn>>();
        public static Func<T, TReturn> GetOrAddCache(PropertyInfo propertyInfo,int key)
        {
            if (_Functions.TryGetValue(key, out Func<T, TReturn> func))
                return func;
            return (_Functions[key] = GetFunction(propertyInfo));
        }

        /// <![CDATA[
        ///     object GetterFunction(MyClass i) => i.MyProperty1 as object ;  
        /// ]]>
        private static Func<T, TReturn> GetFunction(PropertyInfo prop)
        {
            var instance = Expression.Parameter(prop.DeclaringType, "i");
            var property = Expression.Property(instance, prop);
            var convert = Expression.TypeAs(property, typeof(TReturn));
            var lambda = Expression.Lambda<Func<T, TReturn>>(convert, instance);
            return lambda.Compile();
        }
    }

    public static partial class ValueObjectHelper
    {
        public static Dictionary<string, object> GetObjectValues<T>(this T instance) => instance == null
                ? null
                : instance.GetType().GetPropertiesFromCache().ToDictionary(key => key.Name, value => value.GetObjectValue(instance));

        public static object GetObjectValue<T>(this PropertyInfo propertyInfo, T instance)
        {
            var key = propertyInfo.MetadataToken;
            return ValueObjectCache<T,object>.GetOrAddCache(propertyInfo,key)(instance);
        }
    }
}

