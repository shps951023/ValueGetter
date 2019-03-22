using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;
using System;
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
        private static MyClass _Data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };

        [Benchmark()]
        public  void Reflection() => _Data.GetObjectValuesByReflection();
        [Benchmark()]
        public  void ReflectionCacheProperties() => _Data.GetObjectValuesByReflectionCache();
        [Benchmark()]
        public  void EmitCache() => _Data.GetObjectValues();
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
            var type = instance.GetType();
            var props = type.GetProperties();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance));
        }

        public static Dictionary<string, object> GetObjectValuesByReflectionCache<T>(this T instance)
        {
            var type = instance.GetType();
            var props = type.GetPropertiesFromCache();
            return props.ToDictionary(key => key.Name, value => value.GetValue(instance));
        }
    }
}
