﻿// See https://aka.ms/new-console-template for more information
// <auto-generated/>
using global::System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using FastExpressionCompiler;
using Sigil;

public class Program
{
  public static void Main()
  {
    BenchmarkRunner.Run<Benchmark>();
    //new Benchmark().SlowReflection();
    //new Benchmark().FastReflection();
  }
}


[SimpleJob(RuntimeMoniker.Net48, baseline: true)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net50)]
[SimpleJob(RuntimeMoniker.Net60)]
[SimpleJob(RuntimeMoniker.Net70)]
[MemoryDiagnoser()]
public class Benchmark
{
  private readonly C1 _c1;
  private readonly Func<C1, C2> _dynamicMethod, _delegate, _builtFromExpression, _fastCompiledExpression;

  private readonly PropertyInfo setInt,
    setDecimal,
    setString;

  private readonly PropertyInfo getInt,
    getDecimal,
    getString;
  
  private readonly FieldInfo sourceStringField, destStringField;

  private readonly object[] args = new object[1];
  public Benchmark()
  {
    _c1 = new C1() {
      Int = 777,
      Decimal = 3.14000000000001m,
      String = "Hello",
      StringField = "World"
    };

    _dynamicMethod = CreateMethod();
    _delegate = MapMethod;
    var variable = Expression.Variable(typeof(C2));
    var arg = Expression.Parameter(typeof(C1));

    Expression<Func<C1, C2>> exp = Expression.Lambda<Func<C1, C2>>(Expression.Block(new [] { variable }, new Expression[] {
      Expression.Assign(variable, Expression.New(typeof(C2))),
      Expression.Assign(Expression.PropertyOrField(variable, "Int"), Expression.PropertyOrField(arg, "Int")),
      Expression.Assign(Expression.PropertyOrField(variable, "Decimal"), Expression.PropertyOrField(arg, "Decimal")),
      Expression.Assign(Expression.PropertyOrField(variable, "StringField"), Expression.PropertyOrField(arg, "StringField")),
      Expression.Assign(Expression.PropertyOrField(variable, "String"), Expression.PropertyOrField(arg, "String")),
      variable
    }), arg);
    _builtFromExpression = exp.Compile();
    _fastCompiledExpression = exp.CompileFast();

    var destType = typeof(C2);
    var sourceType = typeof(C1);
    
    getInt = sourceType.GetProperty("Int");
    getDecimal = sourceType.GetProperty("Decimal");
    getString = sourceType.GetProperty("String");
    
    setInt = destType.GetProperty("Int");
    setDecimal = destType.GetProperty("Decimal");
    setString = destType.GetProperty("String");

    sourceStringField = sourceType.GetField("StringField");
    destStringField = destType.GetField("StringField");
  }

  private Func<C1, C2> CreateMethod()
  {
    var emiter = Emit<Func<C1, C2>>.NewDynamicMethod("MyMethod");
    using (var local = emiter.DeclareLocal<C2>())
    {
      emiter.NewObject<C2>();
      emiter.StoreLocal(local);

      emiter.LoadLocal(local);
      emiter.LoadArgument(0);
      emiter.CallVirtual(typeof(C1).GetProperty("Int")?.GetMethod);
      emiter.CallVirtual(typeof(C2).GetProperty("Int")?.SetMethod);
  
      emiter.LoadLocal(local);
      emiter.LoadArgument(0);
      emiter.CallVirtual(typeof(C1).GetProperty("Decimal")?.GetMethod);
      emiter.CallVirtual(typeof(C2).GetProperty("Decimal")?.SetMethod);
  
      emiter.LoadLocal(local);
      emiter.LoadArgument(0);
      emiter.CallVirtual(typeof(C1).GetProperty("String")?.GetMethod);
      emiter.CallVirtual(typeof(C2).GetProperty("String")?.SetMethod);
  
      emiter.LoadLocal(local);
      emiter.LoadArgument(0);
      emiter.LoadField(typeof(C1).GetField("StringField"));
      emiter.StoreField(typeof(C2).GetField("StringField"));
  
      emiter.LoadLocal(local);
      emiter.Return();
    }
    return emiter.CreateDelegate();
  }
  
  [MethodImpl(MethodImplOptions.NoInlining)]
  private C2 MapMethod(C1 c)
  { 
    C2 result = new C2();
    result.Int = c.Int;
    result.Decimal = c.Decimal;
    result.String = c.String;
    result.StringField = c.StringField;

    return result;
  }

  [Benchmark(Baseline = true)]
  public C2 Baseline() => MapMethod(_c1);

  [Benchmark]
  public C2 Delegate() => _delegate(_c1);
  
  [Benchmark]
  public C2 DynamicMethod() => _dynamicMethod(_c1);

  [Benchmark]
  public C2 CompiledExpression() => _builtFromExpression(_c1);
  
  [Benchmark]
  public C2 FastCompiledExpression() => _fastCompiledExpression(_c1);

  [Benchmark]
  public C2 SlowReflection()
  {
    var type = _c1.GetType();
    var destType = typeof(C2);
    var c2 = Activator.CreateInstance(destType);
    
    foreach (var prop in type.GetProperties())
    {
      destType.GetProperty(prop.Name).SetValue(c2, prop.GetValue(_c1));  
    }
    
    foreach (var prop in type.GetFields())
    {
      destType.GetField(prop.Name).SetValue(c2, prop.GetValue(_c1));  
    }
    return (C2)c2;
  }

  [Benchmark]
  public C2 FastReflection()
  {
    var c2 = Activator.CreateInstance<C2>();

    setInt.SetValue(c2, getInt.GetValue(_c1));
    setDecimal.SetValue(c2, getDecimal.GetValue(_c1));
    setString.SetValue(c2, getString.GetValue(_c1));
    destStringField.SetValue(c2, sourceStringField.GetValue(_c1));

    return c2;
  }
}

public class C1
{
  public int Int { get; set; }
  public decimal Decimal { get; set; }
  public string StringField;
  public string String { get; set; }
}

public class C2
{
  public int Int { get; set; }
  public decimal Decimal { get; set; }
  public string StringField;
  public string String { get; set; }
}


/*
|                 Method |          Job |         Mean |       Error |      StdDev |       Median |  Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------- |------------- |-------------:|------------:|------------:|-------------:|-------:|--------:|-------:|----------:|------------:|
|               Baseline |Framework 4.8 |     7.794 ns |   0.4898 ns |   1.3571 ns |     7.496 ns |   1.00 |    0.00 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |Framework 4.8 |     8.103 ns |   0.2260 ns |   0.4768 ns |     8.054 ns |   1.12 |    0.17 | 0.0089 |      56 B |        1.00 |
|               Delegate |Framework 4.8 |     7.974 ns |   0.2216 ns |   0.4910 ns |     7.940 ns |   1.09 |    0.19 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |Framework 4.8 |    39.070 ns |   0.8423 ns |   1.7016 ns |    39.063 ns |   5.45 |    0.88 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |Framework 4.8 |     7.705 ns |   0.2189 ns |   0.4520 ns |     7.730 ns |   1.07 |    0.18 | 0.0089 |      56 B |        1.00 |
|         SlowReflection |Framework 4.8 | 1,610.503 ns |  32.0085 ns |  63.9245 ns | 1,611.465 ns | 224.01 |   32.66 | 0.0610 |     385 B |        6.88 |
|         FastReflection |Framework 4.8 | 1,051.206 ns |  20.8693 ns |  44.0204 ns | 1,042.916 ns | 144.90 |   21.24 | 0.0477 |     305 B |        5.45 |

|               Baseline |.NET Core 3.1 |     8.708 ns |   0.2633 ns |   0.7682 ns |     8.588 ns |   1.16 |    0.22 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |.NET Core 3.1 |     8.193 ns |   0.2266 ns |   0.5600 ns |     8.254 ns |   1.08 |    0.21 | 0.0089 |      56 B |        1.00 |
|               Delegate |.NET Core 3.1 |     8.108 ns |   0.2269 ns |   0.4736 ns |     8.101 ns |   1.12 |    0.18 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |.NET Core 3.1 |     9.232 ns |   0.5360 ns |   1.5805 ns |     9.259 ns |   1.21 |    0.25 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |.NET Core 3.1 |     9.285 ns |   0.3705 ns |   1.0924 ns |     9.443 ns |   1.21 |    0.20 | 0.0089 |      56 B |        1.00 |
|         SlowReflection |.NET Core 3.1 | 1,396.654 ns |  94.5429 ns | 278.7618 ns | 1,250.371 ns | 189.04 |   52.35 | 0.0610 |     384 B |        6.86 |
|         FastReflection |.NET Core 3.1 |   846.239 ns |  16.9672 ns |  38.9849 ns |   846.052 ns | 114.68 |   20.01 | 0.0477 |     304 B |        5.43 |

|               Baseline |     .NET 5.0 |     6.064 ns |   0.1778 ns |   0.4775 ns |     6.003 ns |   0.80 |    0.14 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 5.0 |     6.892 ns |   0.1918 ns |   0.3098 ns |     6.831 ns |   0.93 |    0.15 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 5.0 |     8.224 ns |   0.2436 ns |   0.7028 ns |     8.164 ns |   1.08 |    0.19 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 5.0 |     7.738 ns |   0.2609 ns |   0.7610 ns |     7.791 ns |   1.02 |    0.20 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 5.0 |     8.002 ns |   0.3092 ns |   0.8969 ns |     7.934 ns |   1.05 |    0.16 | 0.0089 |      56 B |        1.00 |
|         SlowReflection |     .NET 5.0 | 1,080.053 ns |  21.4425 ns |  47.9593 ns | 1,078.085 ns | 146.86 |   21.96 | 0.0610 |     384 B |        6.86 |
|         FastReflection |     .NET 5.0 |   735.505 ns |  14.2707 ns |  21.3598 ns |   729.531 ns |  97.61 |   16.96 | 0.0477 |     304 B |        5.43 |

|               Baseline |     .NET 6.0 |     7.159 ns |   0.2613 ns |   0.7622 ns |     7.019 ns |   0.95 |    0.18 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 6.0 |     7.975 ns |   0.2190 ns |   0.2998 ns |     7.886 ns |   1.04 |    0.20 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 6.0 |     9.212 ns |   0.3180 ns |   0.8970 ns |     9.139 ns |   1.22 |    0.24 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 6.0 |     8.006 ns |   0.2431 ns |   0.7129 ns |     7.881 ns |   1.06 |    0.19 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 6.0 |     7.467 ns |   0.2026 ns |   0.4138 ns |     7.398 ns |   1.04 |    0.16 | 0.0089 |      56 B |        1.00 |
|         SlowReflection |     .NET 6.0 |   790.373 ns |  15.8412 ns |  22.7190 ns |   786.504 ns | 104.22 |   18.81 | 0.0305 |     192 B |        3.43 |
|         FastReflection |     .NET 6.0 |   840.921 ns | 125.6284 ns | 370.4181 ns |   594.881 ns | 103.01 |   44.64 | 0.0172 |     112 B |        2.00 |

|               Baseline |     .NET 7.0 |     9.319 ns |   0.3673 ns |   1.0655 ns |     9.246 ns |   1.23 |    0.25 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 7.0 |    10.247 ns |   0.3229 ns |   0.9367 ns |    10.045 ns |   1.36 |    0.23 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 7.0 |    10.854 ns |   0.5694 ns |   1.6611 ns |    10.593 ns |   1.43 |    0.31 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 7.0 |     8.408 ns |   0.2392 ns |   0.6902 ns |     8.330 ns |   1.10 |    0.18 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 7.0 |     9.014 ns |   0.2325 ns |   0.5526 ns |     8.988 ns |   1.20 |    0.21 | 0.0089 |      56 B |        1.00 |
|         SlowReflection |     .NET 7.0 |   439.142 ns |   8.7813 ns |  15.1474 ns |   436.838 ns |  59.86 |    9.67 | 0.0305 |     192 B |        3.43 |
|         FastReflection |     .NET 7.0 |   250.846 ns |   4.8429 ns |  11.6961 ns |   250.167 ns |  33.11 |    6.15 | 0.0176 |     112 B |        2.00 |






^






















|                 Method |          Job |         Mean |       Error |      StdDev |       Median |  Ratio | RatioSD |   Gen0 | Allocated | Alloc Ratio |
|----------------------- |------------- |-------------:|------------:|------------:|-------------:|-------:|--------:|-------:|----------:|------------:|
|               Baseline |Framework 4.8 |     7.794 ns |   0.4898 ns |   1.3571 ns |     7.496 ns |   1.00 |    0.00 | 0.0089 |      56 B |        1.00 |
|               Baseline |.NET Core 3.1 |     8.708 ns |   0.2633 ns |   0.7682 ns |     8.588 ns |   1.16 |    0.22 | 0.0089 |      56 B |        1.00 |
|               Baseline |     .NET 5.0 |     6.064 ns |   0.1778 ns |   0.4775 ns |     6.003 ns |   0.80 |    0.14 | 0.0089 |      56 B |        1.00 |
|               Baseline |     .NET 6.0 |     7.159 ns |   0.2613 ns |   0.7622 ns |     7.019 ns |   0.95 |    0.18 | 0.0089 |      56 B |        1.00 |
|               Baseline |     .NET 7.0 |     9.319 ns |   0.3673 ns |   1.0655 ns |     9.246 ns |   1.23 |    0.25 | 0.0089 |      56 B |        1.00 |

|               Delegate |Framework 4.8 |     7.974 ns |   0.2216 ns |   0.4910 ns |     7.940 ns |   1.09 |    0.19 | 0.0089 |      56 B |        1.00 |
|               Delegate |.NET Core 3.1 |     8.108 ns |   0.2269 ns |   0.4736 ns |     8.101 ns |   1.12 |    0.18 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 5.0 |     8.224 ns |   0.2436 ns |   0.7028 ns |     8.164 ns |   1.08 |    0.19 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 6.0 |     9.212 ns |   0.3180 ns |   0.8970 ns |     9.139 ns |   1.22 |    0.24 | 0.0089 |      56 B |        1.00 |
|               Delegate |     .NET 7.0 |    10.854 ns |   0.5694 ns |   1.6611 ns |    10.593 ns |   1.43 |    0.31 | 0.0089 |      56 B |        1.00 |

|         SlowReflection |Framework 4.8 | 1,610.503 ns |  32.0085 ns |  63.9245 ns | 1,611.465 ns | 224.01 |   32.66 | 0.0610 |     385 B |        6.88 |
|         SlowReflection |.NET Core 3.1 | 1,396.654 ns |  94.5429 ns | 278.7618 ns | 1,250.371 ns | 189.04 |   52.35 | 0.0610 |     384 B |        6.86 |
|         SlowReflection |     .NET 5.0 | 1,080.053 ns |  21.4425 ns |  47.9593 ns | 1,078.085 ns | 146.86 |   21.96 | 0.0610 |     384 B |        6.86 |
|         SlowReflection |     .NET 6.0 |   790.373 ns |  15.8412 ns |  22.7190 ns |   786.504 ns | 104.22 |   18.81 | 0.0305 |     192 B |        3.43 |
|         SlowReflection |     .NET 7.0 |   439.142 ns |   8.7813 ns |  15.1474 ns |   436.838 ns |  59.86 |    9.67 | 0.0305 |     192 B |        3.43 |

|         FastReflection |Framework 4.8 | 1,051.206 ns |  20.8693 ns |  44.0204 ns | 1,042.916 ns | 144.90 |   21.24 | 0.0477 |     305 B |        5.45 |
|         FastReflection |.NET Core 3.1 |   846.239 ns |  16.9672 ns |  38.9849 ns |   846.052 ns | 114.68 |   20.01 | 0.0477 |     304 B |        5.43 |
|         FastReflection |     .NET 5.0 |   735.505 ns |  14.2707 ns |  21.3598 ns |   729.531 ns |  97.61 |   16.96 | 0.0477 |     304 B |        5.43 |
|         FastReflection |     .NET 6.0 |   840.921 ns | 125.6284 ns | 370.4181 ns |   594.881 ns | 103.01 |   44.64 | 0.0172 |     112 B |        2.00 |
|         FastReflection |     .NET 7.0 |   250.846 ns |   4.8429 ns |  11.6961 ns |   250.167 ns |  33.11 |    6.15 | 0.0176 |     112 B |        2.00 |

|     CompiledExpression |Framework 4.8 |    39.070 ns |   0.8423 ns |   1.7016 ns |    39.063 ns |   5.45 |    0.88 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |.NET Core 3.1 |     9.232 ns |   0.5360 ns |   1.5805 ns |     9.259 ns |   1.21 |    0.25 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 5.0 |     7.738 ns |   0.2609 ns |   0.7610 ns |     7.791 ns |   1.02 |    0.20 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 6.0 |     8.006 ns |   0.2431 ns |   0.7129 ns |     7.881 ns |   1.06 |    0.19 | 0.0089 |      56 B |        1.00 |
|     CompiledExpression |     .NET 7.0 |     8.408 ns |   0.2392 ns |   0.6902 ns |     8.330 ns |   1.10 |    0.18 | 0.0089 |      56 B |        1.00 |

| FastCompiledExpression |Framework 4.8 |     7.705 ns |   0.2189 ns |   0.4520 ns |     7.730 ns |   1.07 |    0.18 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |.NET Core 3.1 |     9.285 ns |   0.3705 ns |   1.0924 ns |     9.443 ns |   1.21 |    0.20 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 5.0 |     8.002 ns |   0.3092 ns |   0.8969 ns |     7.934 ns |   1.05 |    0.16 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 6.0 |     7.467 ns |   0.2026 ns |   0.4138 ns |     7.398 ns |   1.04 |    0.16 | 0.0089 |      56 B |        1.00 |
| FastCompiledExpression |     .NET 7.0 |     9.014 ns |   0.2325 ns |   0.5526 ns |     8.988 ns |   1.20 |    0.21 | 0.0089 |      56 B |        1.00 |

|          DynamicMethod |Framework 4.8 |     8.103 ns |   0.2260 ns |   0.4768 ns |     8.054 ns |   1.12 |    0.17 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |.NET Core 3.1 |     8.193 ns |   0.2266 ns |   0.5600 ns |     8.254 ns |   1.08 |    0.21 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 5.0 |     6.892 ns |   0.1918 ns |   0.3098 ns |     6.831 ns |   0.93 |    0.15 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 6.0 |     7.975 ns |   0.2190 ns |   0.2998 ns |     7.886 ns |   1.04 |    0.20 | 0.0089 |      56 B |        1.00 |
|          DynamicMethod |     .NET 7.0 |    10.247 ns |   0.3229 ns |   0.9367 ns |    10.045 ns |   1.36 |    0.23 | 0.0089 |      56 B |        1.00 |
*/

//2:47:00
//2:54:00