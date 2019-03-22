using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ValueGetter;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Attributes;


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
        private static List<MyClass> _Data = Enumerable.Range(1,100)
            .Select(s=>new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();
        private static List<object> _Data2 = Enumerable.Range(1, 100)
            .Select(s => (object)new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();
        private static List<object> _Data3 = Enumerable.Range(1, 100)
            .Select(s => null as object).ToList();

        [Benchmark()]
        public  void Reflection() => _Data.Select(s=> s.GetObjectValuesByReflection()).ToList();
        [Benchmark()]
        public  void ReflectionCacheProperties() => _Data.Select(s => s.GetObjectValuesByReflectionCache()).ToList(); 
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

    public class Config : ManualConfig
    {
        public const int Iterations = 500;

        public Config()
        {
            Add(ConsoleLogger.Default);

            Add(CsvExporter.Default);
            Add(MarkdownExporter.GitHub);
            Add(HtmlExporter.Default);

            var md = new MemoryDiagnoser();
            Add(md);
            Add(TargetMethodColumn.Method);
            Add(StatisticColumn.Mean);
            //Add(StatisticColumn.StdDev);
            //Add(StatisticColumn.Error);
            Add(BaselineScaledColumn.Scaled);
            Add(md.GetColumnProvider());

            Add(Job.ShortRun
                   .WithLaunchCount(1)
                   .WithWarmupCount(2)
                   .WithUnrollFactor(Iterations)
                   .WithIterationCount(1)
            );
            Set(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));
            SummaryPerType = false;
        }
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
    }
}
