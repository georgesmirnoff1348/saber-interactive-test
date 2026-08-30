**Custom serializer implementation:**

Please provide the following:

1. Provide the ```IListSerializer``` interface implementation 
(you can use any serialization format but you could not utilize third-party libraries that will serialize the full list for you. I.e. it's allowed to utilize third-party libraries for serializing 1 node in particular format):
- provided data structures, class names or namespaces could not be changed;
- solution is allowed to be not thread safe;
- it's guaranteed that list provided as an argument to ```Serialize``` and ```DeepCopy``` function is consistent and doesn't contain any cycles;
- automated testing of your solution will be performed, the resulting rate for the solution will be given based on (in order of priority):
  - tests on correctness of the solution 
  - performance tests 
  - tests on memory consumption

*please note, we have special requirements for performance.
  
2. Write your own test cases for the implementation for ```IListSerializer``` interface.




1

Final implementation - IvanImplementation.cs

Running benchmarks for the solution

dotnet run -c Release --project BenchmarkRunner/BenchmarkRunner.csproj

Result in BenchmarkDotNet.Artifacts

2

Unit tests can be run in Docker

1. SerializeDeserialize_SimpleList_PreservesStructure - Serialize/deserialize a list of 3 nodes with a random link
2. SerializeDeserialize_ComplexRandomLinks_PreservesStructure - Complex random links including self-references (5 nodes)
3. SerializeDeserialize_AllRandomsNull_PreservesStructure - List where all random links are null (10 nodes)
4. DeepCopy_ModifiedCopy_DoesNotAffectOriginal - Check that DeepCopy creates an independent copy
5. SerializeDeserialize_LargeStringData_PreservesData - Large strings (10K and 50K characters)
6. DeepCopy_LargeStringData_PreservesData - DeepCopy with large strings and self-reference
7. SerializeDeserialize_RoundTrip_ProducesSameList - Double serialization/deserialization (20 nodes)
8. PerformanceTest_VeryLongList - Performance: 400K nodes
9. DeepCopy_Null_ReturnsNull - DeepCopy of null returns null
10. Serialize_NullNode_DoesNotThrow - Serializing null does not throw an exception
11. Deserialize_EmptyStream_ReturnsNull - Deserializing an empty stream returns null
12. Deserialize_NullStream_ThrowsArgumentNullException - Deserializing a null stream throws an exception
13. Serialize_NullStream_ThrowsArgumentNullException - Serializing to a null stream throws an exception
14. SerializeDeserialize_SingleNodeWithNullData_PreservesStructure - Single node with Data = null
15. SerializeDeserialize_MultipleNodesWithNullData_PreservesStructure - Multiple nodes with null Data
16. SerializeDeserialize_SingleNode_PreservesStructure - Single node with data
17. SerializeDeserialize_SingleNodeWithSelfRandom_PreservesStructure - Single node with Random on itself
18. SerializeDeserialize_TwoNodes_PreservesStructure - Two nodes
