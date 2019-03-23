

[English](Readme.md) | [中文](Readme_zh-cn.md) 


---

[![NuGet](https://img.shields.io/nuget/v/ValueGetter.svg)](https://www.nuget.org/packages/ValueGetter)
![](https://img.shields.io/nuget/dt/ValueGetter.svg)

---

### Online Demo

[ValueGetter Demo  | .NET Fiddle](https://dotnetfiddle.net/wZUhgw)

---

### Feature:
- Mini (you can just copy/paste a [ValueGetter.cs](ValueGetter/ValueGetter.cs) file to project)
- Faster Than Reflection GeValue (Click To [BenchmarkDotNet])
- Support `net40;net45;netstandard2.0;` frameworks
- KISS Style

### Installation

You can install the package [from NuGet](https://www.nuget.org/packages/ValueGetter) using the Visual Studio Package Manager or NuGet UI:

```cmd
PM> install-package ValueGetter
```

or the `dotnet` command line:

```cmd
dotnet add package ValueGetter
```

### GetStart:

Get all the property values of object, return `Dictionary<string,object>` type
```C#
    var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
    var result = data.GetObjectValues();
    //Result:
    Assert.AreEqual(123, result["MyProperty1"]);
    Assert.AreEqual("test", result["MyProperty2"]);
```

Get all properties ToString value of the object, return `Dictionary<string,string>` type
```C#
    var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
    var result = data.GetToStringValues();
    //Result:
    Assert.AreEqual("123", result["MyProperty1"]);
    Assert.AreEqual("test", result["MyProperty2"]);
```

Get single property value of object,return `object` type.
```C#
    var data = new MyClass() { MyProperty1 = 123 };
    var prop = data.GetType().GetProperty("MyProperty1");
    var result = prop.GetObjectValue(data);
    //Result:
    Assert.AreEqual(123, result);
```

Get single property ToString value of object,return `string` type.
```C#
    var data = new MyClass() { MyProperty1 = 123 };
    var prop = data.GetType().GetProperty("MyProperty1");
    var result = prop.GetToStringValue(data);
    //Result:
    Assert.AreEqual("123", result);
```


### BenchmarkDotNet 

Logic:
```C#
public class BenchmarkBase
{
    private static List<MyClass> Data = Enumerable.Range(1,100).Select(s=>new MyClass() { MyProperty1 = 123, MyProperty2 = "test" }).ToList();

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
        return props.ToDictionary(key => key.Name, value => value.GetValue(instance).ToString());
    }).ToList();

    [Benchmark()]
    public void GetObjectValues() => Data.Select(s => s.GetObjectValues()).ToList();

    [Benchmark()]
    public void GetObjectToStringValues() => Data.Select(s => s.GetToStringValues()).ToList();
}
```

Result:

``` ini
BenchmarkDotNet=v0.11.1, OS=Windows 10.0.17134.648 (1803/April2018Update/Redstone4)
Intel Core i7-7700 CPU 3.60GHz (Kaby Lake), 1 CPU, 8 logical and 4 physical cores
  [Host]   : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3362.0
  ShortRun : .NET Framework 4.7.2 (CLR 4.0.30319.42000), 64bit RyuJIT-v4.7.3362.0
```
|                  Method |     Mean |   Gen 0 | Allocated |
|------------------------ |---------:|--------:|----------:|
|         GetObjectValues | 32.93 us |  9.8750 |  40.51 KB |
| GetObjectToStringValues | 38.23 us | 10.0625 |  41.29 KB |
|              Reflection | 54.40 us | 10.0625 |  41.29 KB |
|      ReflectionToString | 60.24 us | 10.8125 |  44.42 KB |