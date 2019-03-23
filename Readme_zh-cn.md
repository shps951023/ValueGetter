

[English](Readme.md) | [中文](Readme_zh-cn.md) 

---

### Feature:
- 輕量 (只需要複製貼上 [ValueGetter.cs](ValueGetter/ValueGetter.cs) 到你的專案)
- 沒有第三方套件依賴
- 比Reflection GeValue效率高(查看 [BenchmarkDotNet] )
- 支援版本 `net40;net45;netstandard2.0;` frameworks

### 使用方式:

獲取物件的所有屬性值,返回`Dictionary<string,object>`型態
```C#
var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
var result = data.GetObjectValues();
Assert.AreEqual(123, result["MyProperty1"]);
Assert.AreEqual("test", result["MyProperty2"]);
```

獲取物件的所有屬性字串值(底層幫忙呼叫ToString),返回`Dictionary<string,string>`型態
```C#
var data = new MyClass() { MyProperty1 = 123, MyProperty2 = "test" };
var result = data.GetToStringValues();
Assert.AreEqual("123", result["MyProperty1"]);
Assert.AreEqual("test", result["MyProperty2"]);
```

獲取單一屬性的值,返回`Object`型態
```C#
var data = new MyClass() { MyProperty1 = 123 };
var prop = data.GetType().GetProperty("MyProperty1");
var result = prop.GetObjectValue(data);
Assert.AreEqual(123, result);
```

獲取單一屬性的值(底層幫忙呼叫ToString),返回`String`型態
```C#
var data = new MyClass() { MyProperty1 = 123 };
var prop = data.GetType().GetProperty("MyProperty1");
var result = prop.GetToStringValue(data);
Assert.AreEqual("123", result);
```


### BenchmarkDotNet 效能測試

邏輯:
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

結果:

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