using SerializerTests.Implementations;
using SerializerTests.Interfaces;
using SerializerTests.Nodes;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace SerializerTests.Tests
{
    public class ListSerializerTests
    {
        // Helper methods
        private ListNode CreateList(int count, bool randomize = true)
        {
            if (count == 0) return null!;

            var nodes = new ListNode[count];
            for (int i = 0; i < count; i++)
            {
                nodes[i] = new ListNode
                {
                    Data = $"Node{i}",
                    Previous = i > 0 ? nodes[i - 1] : null
                };
            }

            for (int i = 0; i < count - 1; i++)
            {
                nodes[i].Next = nodes[i + 1];
            }

            if (randomize)
            {
                var rand = new Random(42); // Fixed seed for reproducibility
                for (int i = 0; i < count; i++)
                {
                    // 70% chance to have a Random link
                    if (rand.NextDouble() < 0.7)
                    {
                        nodes[i].Random = nodes[rand.Next(0, count)];
                    }
                }
            }

            return nodes[0];
        }

        private bool ListsAreEqual(ListNode list1, ListNode list2)
        {
            if (list1 == null && list2 == null)
                return true;
            if (list1 == null || list2 == null)
                return false;

            var nodes1 = new List<ListNode>();
            var nodes2 = new List<ListNode>();
            var dict1 = new Dictionary<ListNode, int>();
            var dict2 = new Dictionary<ListNode, int>();

            // Collect all nodes from list1
            var current = list1;
            int index = 0;
            while (current != null)
            {
                if (dict1.ContainsKey(current))
                    return false; // Cycle detected
                dict1[current] = index++;
                nodes1.Add(current);
                current = current.Next;
            }

            // Collect all nodes from list2
            current = list2;
            index = 0;
            while (current != null)
            {
                if (dict2.ContainsKey(current))
                    return false; // Cycle detected
                dict2[current] = index++;
                nodes2.Add(current);
                current = current.Next;
            }

            if (nodes1.Count != nodes2.Count)
                return false;

            // Compare nodes
            for (int i = 0; i < nodes1.Count; i++)
            {
                if (nodes1[i].Data != nodes2[i].Data)
                    return false;

                // Check Previous
                if (nodes1[i].Previous == null)
                {
                    if (nodes2[i].Previous != null)
                        return false;
                }
                else
                {
                    if (nodes2[i].Previous == null)
                        return false;
                    int idx1 = dict1[nodes1[i].Previous];
                    int idx2 = dict2[nodes2[i].Previous];
                    if (idx1 != idx2)
                        return false;
                }

                // Check Next
                if (nodes1[i].Next == null)
                {
                    if (nodes2[i].Next != null)
                        return false;
                }
                else
                {
                    if (nodes2[i].Next == null)
                        return false;
                    int idx1 = dict1[nodes1[i].Next];
                    int idx2 = dict2[nodes2[i].Next];
                    if (idx1 != idx2)
                        return false;
                }

                // Check Random
                if (nodes1[i].Random == null)
                {
                    if (nodes2[i].Random != null)
                        return false;
                }
                else
                {
                    if (nodes2[i].Random == null)
                        return false;
                    int idx1 = dict1[nodes1[i].Random];
                    int idx2 = dict2[nodes2[i].Random];
                    if (idx1 != idx2)
                        return false;
                }
            }

            return true;
        }

        private int GetListLength(ListNode head)
        {
            int count = 0;
            var current = head;
            while (current != null)
            {
                count++;
                current = current.Next;
            }
            return count;
        }

        [Fact]
        public async Task SerializeDeserialize_SimpleList_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var head = CreateList(3, randomize: false);
            head.Random = head.Next!.Next; // Head points to tail

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(head, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(head, result));
        }

        [Fact]
        public async Task SerializeDeserialize_ComplexRandomLinks_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();

            // Create a specific pattern: 5 nodes with interesting Random links
            var nodes = new ListNode[5];
            for (int i = 0; i < 5; i++)
            {
                nodes[i] = new ListNode { Data = $"Node{i}", Previous = i > 0 ? nodes[i - 1] : null };
                if (i > 0) nodes[i - 1].Next = nodes[i];
            }

            // Create specific Random pattern
            nodes[0].Random = nodes[2];      // Head -> middle
            nodes[1].Random = nodes[0];      // Second -> head
            nodes[2].Random = nodes[4];      // Middle -> tail
            nodes[3].Random = nodes[3];      // Fourth -> self (self-reference!)
            nodes[4].Random = nodes[1];      // Tail -> second

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(nodes[0], stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(nodes[0], result));
        }

        [Fact]
        public async Task SerializeDeserialize_AllRandomsNull_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var head = CreateList(10, randomize: false);

            // Ensure all Randoms are null
            var current = head;
            while (current != null)
            {
                current.Random = null;
                current = current.Next;
            }

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(head, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(head, result));
        }

        [Fact]
        public async Task DeepCopy_ModifiedCopy_DoesNotAffectOriginal()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var head = CreateList(5, randomize: true);

            // Act
            var copy = await serializer.DeepCopy(head);

            // Modify the copy
            var current = copy;
            while (current != null)
            {
                current.Data = "MODIFIED";
                current = current.Next;
            }

            // Assert
            current = head;
            while (current != null)
            {
                Assert.NotEqual("MODIFIED", current.Data);
                current = current.Next;
            }
        }

        [Fact]
        public async Task SerializeDeserialize_LargeStringData_PreservesData()
        {
            // Arrange
            var serializer = new IvanImplementation();

            var largeString1 = new string('A', 10000);
            var largeString2 = new string('B', 50000);

            var node1 = new ListNode { Data = largeString1 };
            var node2 = new ListNode { Data = largeString2, Previous = node1 };
            node1.Next = node2;
            node1.Random = node2;
            node2.Random = node1;

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(node1, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(node1, result));
            Assert.Equal(largeString1, result.Data);
            Assert.Equal(largeString2, result.Next!.Data);
        }

        [Fact]
        public async Task DeepCopy_LargeStringData_PreservesData()
        {
            // Arrange
            var serializer = new IvanImplementation();

            var largeString1 = new string('X', 15000);
            var largeString2 = new string('Y', 25000);

            var node1 = new ListNode { Data = largeString1 };
            var node2 = new ListNode { Data = largeString2, Previous = node1 };
            node1.Next = node2;
            node2.Random = node2; // Self-reference

            // Act
            var result = await serializer.DeepCopy(node1);

            // Assert
            Assert.True(ListsAreEqual(node1, result));
            Assert.Equal(largeString1, result.Data);
            Assert.Equal(largeString2, result.Next!.Data);
        }

        [Fact]
        public async Task SerializeDeserialize_RoundTrip_ProducesSameList()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var original = CreateList(20, randomize: true);

            using var stream = new MemoryStream();

            // Act - First round trip
            await serializer.Serialize(original, stream);
            stream.Position = 0;
            var first = await serializer.Deserialize(stream);

            // Act - Second round trip (serialize the deserialized version)
            stream.SetLength(0);
            await serializer.Serialize(first, stream);
            stream.Position = 0;
            var second = await serializer.Deserialize(stream);

            // Assert - All three should be equal
            Assert.True(ListsAreEqual(original, first));
            Assert.True(ListsAreEqual(first, second));
            Assert.True(ListsAreEqual(original, second));
        }

        [Fact]
        public async Task PerformanceTest_VeryLongList()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var count = 400_000;
            var head = CreateList(count, randomize: true);

            var sw = new Stopwatch();

            // Deserialize
            using (var stream = new MemoryStream())
            {
                await serializer.Serialize(head, stream);
                stream.Position = 0;

                sw.Restart();
                var result = await serializer.Deserialize(stream);
                sw.Stop();

                Console.WriteLine($"[{serializer.GetType().Name}] Deserialized {count} nodes in {sw.ElapsedMilliseconds}ms");

                Assert.True(ListsAreEqual(head, result));
            }
        }

        [Fact]
        public async Task DeepCopy_Null_ReturnsNull()
        {
            // Arrange
            var serializer = new IvanImplementation();

            // Act
            var result = await serializer.DeepCopy(null);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Serialize_NullNode_DoesNotThrow()
        {
            // Arrange
            var serializer = new IvanImplementation();

            // Act & Assert - Should not throw
            using var stream = new MemoryStream();
            await serializer.Serialize(null, stream);

            // Stream should be empty or contain only marker for null
            Assert.Equal(0, stream.Length);
        }

        [Fact]
        public async Task Deserialize_EmptyStream_ReturnsNull()
        {
            // Arrange
            var serializer = new IvanImplementation();
            using var stream = new MemoryStream();

            // Act
            var result = await serializer.Deserialize(stream);

            // Assert - Should return null or an empty list node
            Assert.Null(result);
        }

        [Fact]
        public async Task Deserialize_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            var serializer = new IvanImplementation();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await serializer.Deserialize(null!);
            });
        }

        [Fact]
        public async Task Serialize_NullStream_ThrowsArgumentNullException()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var head = CreateList(1, randomize: false);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await serializer.Serialize(head, null!);
            });
        }

        [Fact]
        public async Task SerializeDeserialize_SingleNodeWithNullData_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var node = new ListNode { Data = null };

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(node, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.NotNull(result);
            Assert.Null(result.Data);
            Assert.Null(result.Next);
            Assert.Null(result.Previous);
            Assert.Null(result.Random);
        }

        [Fact]
        public async Task SerializeDeserialize_MultipleNodesWithNullData_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var node1 = new ListNode { Data = null };
            var node2 = new ListNode { Data = null, Previous = node1 };
            var node3 = new ListNode { Data = "Not Null", Previous = node2 };
            node1.Next = node2;
            node2.Next = node3;
            node1.Random = node3;

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(node1, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(node1, result));
            Assert.Null(result.Data);
            Assert.Null(result.Next!.Data);
            Assert.Equal("Not Null", result.Next.Next!.Data);
        }

        [Fact]
        public async Task SerializeDeserialize_SingleNode_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var singleNode = new ListNode { Data = "Single" };

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(singleNode, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Single", result.Data);
            Assert.Null(result.Next);
            Assert.Null(result.Previous);
            Assert.Null(result.Random);
        }

        [Fact]
        public async Task SerializeDeserialize_SingleNodeWithSelfRandom_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var singleNode = new ListNode { Data = "SelfRef" };
            singleNode.Random = singleNode; // Self-reference

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(singleNode, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("SelfRef", result.Data);
            Assert.Null(result.Next);
            Assert.Null(result.Previous);
            Assert.Same(result, result.Random); // Should reference itself
        }


        [Fact]
        public async Task SerializeDeserialize_TwoNodes_PreservesStructure()
        {
            // Arrange
            var serializer = new IvanImplementation();
            var node1 = new ListNode { Data = "First" };
            var node2 = new ListNode { Data = "Second", Previous = node1 };
            node1.Next = node2;

            using var stream = new MemoryStream();

            // Act
            await serializer.Serialize(node1, stream);
            stream.Position = 0;
            var result = await serializer.Deserialize(stream);

            // Assert
            Assert.True(ListsAreEqual(node1, result));
            Assert.Equal("First", result.Data);
            Assert.Equal("Second", result.Next!.Data);
            Assert.Null(result.Next.Next);
        }
    }
}
