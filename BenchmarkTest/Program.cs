using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueGetter;
using FastMember;


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
        private static List<MyClass> Data = Enumerable.Range(1,10).Select(s=>new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();

        [Benchmark()]
        public void FastMember() => Data.Select(instance => {
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => ObjectAccessor.Create(instance)[value.Name]);
        }).ToList();

        [Benchmark()]
        public void FastMemberToString() => Data.Select(instance => {
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => ObjectAccessor.Create(instance)[value.Name].ToString());
        }).ToList();

        [Benchmark()]
        public  void Reflection() => Data.Select(instance => {
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance));
        }).ToList();

        [Benchmark()]
        public void ReflectionToString() => Data.Select(instance => {
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance)?.ToString());
        }).ToList();

        [Benchmark()]
        public void GetObjectValues() => Data.Select(s => s.GetObjectValues()).ToList();

        [Benchmark()]
        public void GetObjectToStringValues() => Data.Select(s => s.GetToStringValues()).ToList();
    }

    public class MyClass
    {
        public int MyProperty1 { get; set; }
        public string MyProperty2 { get; set; }
    }
}
