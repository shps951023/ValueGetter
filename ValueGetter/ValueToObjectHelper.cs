using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ValueGetter
{
    public static partial class ValueObjectHelper
    {
        private static readonly ConcurrentDictionary<string, Func<object, object>> _Functions = new ConcurrentDictionary<string, Func<object, object>>();

        public static Dictionary<string, object> GetObjectValues<T>(this T instance)
        {
            var type = instance.GetType();
            var props = type.GetPropertiesFromCache();
            return props.ToDictionary(key=>key.Name,value=>value.GetObjectValue(instance));
        }

        public static object GetObjectValue<T>(this PropertyInfo propertyInfo, T instance)
        {
            if (instance == null) return null;
            if (propertyInfo == null) throw new ArgumentNullException($"{nameof(propertyInfo)} is null");

            var type = propertyInfo.DeclaringType;
            var key = $"{type.TypeHandle.Value.ToString()}|{propertyInfo.MetadataToken.ToString()}";

            Func<object, object> function = null;
            if (_Functions.TryGetValue(key, out Func<object, object> func))
                function = func;
            else
            {
                function = GetFunction(propertyInfo);
                _Functions[key] = function;
            }

            return function(instance);
        }

        private static Func<object, object> GetFunction(PropertyInfo prop)
        {
            var type = prop.DeclaringType;
            var propGetMethod = prop.GetGetMethod(nonPublic: true);
            var propType = prop.PropertyType;

            var methodName = $"m_{Guid.NewGuid().ToString("N")}";
            var dynamicMethod = new DynamicMethod(methodName, typeof(object), new Type[] { typeof(object) }, type.Module);

            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            LocalBuilder local0 = iLGenerator.DeclareLocal(propType);

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Castclass, type);
            iLGenerator.Emit(OpCodes.Call, propGetMethod);
            iLGenerator.Emit(OpCodes.Box, propType);
            iLGenerator.Emit(OpCodes.Ret);

            return (Func<object, object>)dynamicMethod.CreateDelegate(typeof(Func<object, object>));
        }
    }
}

