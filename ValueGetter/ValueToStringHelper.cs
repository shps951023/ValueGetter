using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace ValueGetter
{
    public static partial class ToStringValueHelper
    {
        private static readonly ConcurrentDictionary<string, Func<object, string>> _Functions = new ConcurrentDictionary<string, Func<object, string>>();

        public static IEnumerable<string> GetToStringValues<T>(this T instance)
        {
            var type = instance.GetType();
            var props = type.GetPropertiesFromCache();
            return props.Select(s => s.GetToStringValue(instance));
        }

        public static string GetToStringValue<T>(this PropertyInfo propertyInfo, T instance)
        {
            if (instance == null) return null;
            if (propertyInfo == null) throw new ArgumentNullException($"{nameof(propertyInfo)} is null");

            var type = propertyInfo.DeclaringType;
            var key = $"{type.TypeHandle.Value.ToString()}|{propertyInfo.MetadataToken.ToString()}";

            Func<object, string> function = null;
            if (_Functions.TryGetValue(key, out Func<object, string> func))
                function = func;
            else
            {
                function = GetFunction(propertyInfo);
                _Functions[key] = function;
            }

            return function(instance);
        }

        public static Func<object, string> GetFunction(PropertyInfo prop)
        {
            var type = prop.DeclaringType;
            var propGetMethod = prop.GetGetMethod(nonPublic: true);
            var propType = prop.PropertyType;

            var methodName = $"m_{Guid.NewGuid().ToString("N")}";
            var dynamicMethod = new DynamicMethod(methodName, typeof(string), new Type[] { typeof(object) }, type.Module);
            var toStringMethod = propType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(p => p.Name == "ToString").First();


            ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
            LocalBuilder local0 = iLGenerator.DeclareLocal(propType);

            iLGenerator.Emit(OpCodes.Ldarg_0);
            iLGenerator.Emit(OpCodes.Castclass, type);
            iLGenerator.Emit(OpCodes.Call, propGetMethod);
            iLGenerator.Emit(OpCodes.Stloc, local0);
            iLGenerator.Emit(OpCodes.Ldloca_S, local0);
            iLGenerator.Emit(OpCodes.Call, toStringMethod);
            iLGenerator.Emit(OpCodes.Ret);

            return (Func<object, string>)dynamicMethod.CreateDelegate(typeof(Func<object, string>));
        }
    }
}

