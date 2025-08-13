```

BenchmarkDotNet v0.13.12, Windows 11 (10.0.26100.1742)
Unknown processor
.NET SDK 9.0.300
  [Host]     : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.16 (8.0.1625.21506), X64 RyuJIT AVX2


```
| Method                                | Mean       | Error     | StdDev    | Gen0    | Gen1   | Allocated |
|-------------------------------------- |-----------:|----------:|----------:|--------:|-------:|----------:|
| FindCountyByCoordinate_SinglePoint    |   1.584 μs | 0.0113 μs | 0.0106 μs |  0.1640 |      - |     776 B |
| FindCountyByCoordinate_MultiplePoints |  38.549 μs | 0.6268 μs | 0.5863 μs |  3.6621 |      - |   17352 B |
| FindCountyByCoordinate_RandomPoints   |  14.403 μs | 0.1820 μs | 0.1520 μs |  7.0190 | 0.0305 |   33152 B |
| FindCountyByCoordinate_HotSpotPoints  | 113.069 μs | 1.3445 μs | 1.1919 μs | 13.6719 |      - |   64680 B |
