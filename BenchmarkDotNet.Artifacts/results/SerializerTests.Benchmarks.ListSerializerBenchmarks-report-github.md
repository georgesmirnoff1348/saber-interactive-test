```

BenchmarkDotNet v0.15.8, Linux Ubuntu 24.04.4 LTS (Noble Numbat)
AMD Ryzen 7 2700 3.19GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.313
  [Host]     : .NET 9.0.15 (9.0.15, 9.0.1526.17522), X64 RyuJIT x86-64-v3
  Job-YFEFPZ : .NET 9.0.15 (9.0.15, 9.0.1526.17522), X64 RyuJIT x86-64-v3

IterationCount=10  WarmupCount=3  

```
| Method      | ListSize | SerializerType     | Mean     | Error    | StdDev  | Gen0       | Gen1      | Gen2     | Allocated |
|------------ |--------- |------------------- |---------:|---------:|--------:|-----------:|----------:|---------:|----------:|
| Serialize   | 1000000  | IvanImplementation | 467.9 ms | 10.46 ms | 6.23 ms |          - |         - |        - |  95.05 MB |
| Deserialize | 1000000  | IvanImplementation | 551.0 ms |  9.08 ms | 5.40 ms | 15000.0000 | 7000.0000 |        - | 106.79 MB |
| DeepCopy    | 1000000  | IvanImplementation | 263.6 ms |  5.03 ms | 3.33 ms |  7666.6667 | 4000.0000 | 333.3333 |  45.78 MB |
