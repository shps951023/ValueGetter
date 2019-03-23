using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ValueGetter
{
    public static partial class PropertyCacheHelper
    {
        //private static readonly ConcurrentDictionary<RuntimeTypeHandle, IList<PropertyInfo>> _TypePropertiesCache = new ConcurrentDictionary<RuntimeTypeHandle, IList<PropertyInfo>>();
        private static readonly Dictionary<RuntimeTypeHandle, IList<PropertyInfo>> _TypePropertiesCache 
            = new Dictionary<RuntimeTypeHandle, IList<PropertyInfo>>();

        public static IList<PropertyInfo> GetPropertiesFromCache(this Type type)
        {
            if (_TypePropertiesCache.TryGetValue(type.TypeHandle, out IList<PropertyInfo> pis))
                return pis;
            return _TypePropertiesCache[type.TypeHandle] = type.GetProperties().ToList();
        }
    }
}
