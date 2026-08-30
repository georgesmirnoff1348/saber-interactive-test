1

Итоговая имплементация - IvanImplementation.cs

Запуск бенчмарков для решения

dotnet run -c Release --project BenchmarkRunner/BenchmarkRunner.csproj

Результат в BenchmarkDotNet.Artifacts



2

Юнит-Тесты можно запустить в докере

  1. SerializeDeserialize_SimpleList_PreservesStructure - Сериализация/десериализация списка из 3 узлов с Random ссылкой
  2. SerializeDeserialize_ComplexRandomLinks_PreservesStructure - Сложные Random ссылки включая self-reference (5 узлов)
  3. SerializeDeserialize_AllRandomsNull_PreservesStructure - Список где все Random = null (10 узлов)
  4. DeepCopy_ModifiedCopy_DoesNotAffectOriginal - Проверка что DeepCopy создаёт независимую копию
  5. SerializeDeserialize_LargeStringData_PreservesData - Большие строки (10K и 50K символов)
  6. DeepCopy_LargeStringData_PreservesData - DeepCopy с большими строками и self-reference
  7. SerializeDeserialize_RoundTrip_ProducesSameList - Двойная сериализация/десериализация (20 узлов)
  8. PerformanceTest_VeryLongList - Производительность: 400K узлов
  9. DeepCopy_Null_ReturnsNull - DeepCopy от null возвращает null
  10. Serialize_NullNode_DoesNotThrow - Сериализация null не выбрасывает исключение
  11. Deserialize_EmptyStream_ReturnsNull - Десериализация пустого стрима возвращает null
  12. Deserialize_NullStream_ThrowsArgumentNullException - Десериализация null стрима выбрасывает исключение
  13. Serialize_NullStream_ThrowsArgumentNullException - Сериализация в null стрим выбрасывает исключение
  14. SerializeDeserialize_SingleNodeWithNullData_PreservesStructure - Один узел с Data = null
  15. SerializeDeserialize_MultipleNodesWithNullData_PreservesStructure - Несколько узлов с null Data
  16. SerializeDeserialize_SingleNode_PreservesStructure - Один узел с данными
  17. SerializeDeserialize_SingleNodeWithSelfRandom_PreservesStructure - Один узел с Random на себя
  18. SerializeDeserialize_TwoNodes_PreservesStructure - Два узла