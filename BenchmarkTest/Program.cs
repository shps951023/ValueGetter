using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueGetter;


namespace BenchmarkTest
{
    public  static class Program
    {
        public static void Main(string[] args)
        {
            new BenchmarkSwitcher(typeof(Program).Assembly).Run(args, new Config());
            Console.Read();
        }
    }

    public class BenchmarkBase
    {
        private static List<MyClass> _Data = Enumerable.Range(1,1)
            .Select(s=>new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();
        private static List<object> _Data2 = Enumerable.Range(1, 1)
            .Select(s => (object)new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();
        private static List<object> _Data3 = Enumerable.Range(1, 1)
            .Select(s => null as object).ToList();

        [Benchmark()]
        public  void Reflection() => _Data.Select(s=> s.GetObjectValuesByReflection()).ToList();
        [Benchmark()]
        public  void ReflectionCacheProperties() => _Data.Select(s => s.GetObjectValuesByReflectionCache()).ToList();
        [Benchmark()]
        public void ReflectionToString() => _Data.Select(s => s.GetObjectValuesByReflectionToString()).ToList();
        [Benchmark()]
        public void ReflectionCachePropertiesToString() => _Data.Select(s => s.GetObjectValuesByReflectionCacheToString()).ToList();
        [Benchmark()]
        public  void CompilerFunctionAndCache() => _Data.Select(s => s.GetObjectValues()).ToList();
        [Benchmark()]
        public void CompilerAndCacheAndToString() => _Data.Select(s => s.GetToStringValues()).ToList();
        [Benchmark()]
        public void CompilerFunctionAndCacheAndDynamicType() => _Data2.Select(s => s.GetObjectValues()).ToList();
        [Benchmark()]
        public void CompilerAndCacheAndToStringAndDynamicType() => _Data2.Select(s => s.GetToStringValues()).ToList();
        [Benchmark()]
        public void Null() => _Data3.Select(s => s.GetObjectValues()).ToList();
    }


    public class MyClass
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }

    public static class ValueGetterReflection
    {
        public static Dictionary<string, object> GetObjectValuesByReflection<T>(this T instance)
        {
            if (instance == null) return null;
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance));
        }

        public static Dictionary<string, object> GetObjectValuesByReflectionCache<T>(this T instance)
        {
            if (instance == null) return null;
            var type = instance.GetType();
            var props = type.GetPropertiesFromCache();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance));
        }

        public static Dictionary<string, string> GetObjectValuesByReflectionToString<T>(this T instance)
        {
            if (instance == null) return null;
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance).ToString());
        }

        public static Dictionary<string, string> GetObjectValuesByReflectionCacheToString<T>(this T instance)
        {
            if (instance == null) return null;
            var type = instance.GetType();
            var props = type.GetPropertiesFromCache();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance).ToString());
        }
    }
}
