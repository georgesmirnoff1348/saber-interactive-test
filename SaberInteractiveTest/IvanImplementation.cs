using SerializerTests.Interfaces;
using SerializerTests.Nodes;
using System.Buffers;
using System.Text;
namespace SerializerTests.Implementations
{
    public class IvanImplementation : IListSerializer
    {
        private static readonly Encoding Utf8 = Encoding.UTF8;

        public IvanImplementation()
        {

        }

        public Task<ListNode> DeepCopy(ListNode node)
        {
            if (node == null)
                return Task.FromResult<ListNode>(null);

            ListNode currentOriginal = node;
            while (currentOriginal != null)
            {
                ListNode cloned = new ListNode
                {
                    Data = currentOriginal.Data
                };

                cloned.Next = currentOriginal.Next;
                currentOriginal.Next = cloned;
                currentOriginal = cloned.Next;
            }

            ListNode resultHead = node.Next;
            currentOriginal = node;
            while (currentOriginal != null)
            {
                if (currentOriginal.Random != null)
                    currentOriginal.Next!.Random = currentOriginal.Random.Next;
                currentOriginal = currentOriginal.Next?.Next;
            }

            currentOriginal = node;
            ListNode prevCopy = null;
            while (currentOriginal != null)
            {
                ListNode clonedNode = currentOriginal.Next!;
                clonedNode.Previous = prevCopy;

                currentOriginal.Next = clonedNode.Next;
                if (clonedNode.Next != null)
                    clonedNode.Next = clonedNode.Next.Next;

                prevCopy = clonedNode;
                currentOriginal = currentOriginal.Next;
            }

            return Task.FromResult(resultHead);
        }

        public async Task Serialize(ListNode node, Stream outputStream)
        {
            if (node == null)
                return;
            while (node.Previous is not null)
                node = node.Previous;

            int n = 0;
            ListNode currentOriginal = node;
            while (currentOriginal != null)
            {
                n++;
                currentOriginal = currentOriginal.Next;
            }
            var nodes = new Dictionary<ListNode, int>(n);
            int index = 0;
            currentOriginal = node;
            while (currentOriginal != null)
            {
                nodes.Add(currentOriginal, index++);
                currentOriginal = currentOriginal.Next;
            }

            ListNode current = node;
            while (current != null)
            {
                int byteCount = Utf8.GetByteCount(current.Data ?? string.Empty);
                int totalSize = 4 + byteCount + 4;
                byte[] buffer = ArrayPool<byte>.Shared.Rent(totalSize);
                try
                {
                    var span = buffer.AsSpan(0, totalSize);

                    BitConverter.TryWriteBytes(span.Slice(0, 4), byteCount);

                    Utf8.GetBytes(current.Data ?? string.Empty, span.Slice(4));

                    BitConverter.TryWriteBytes(
                        span.Slice(4 + byteCount, 4),
                        current.Random != null ? nodes[current.Random] : int.MinValue);

                    await outputStream.WriteAsync(buffer.AsMemory(0, totalSize));
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }


                current = current.Next;
            }
        }

        public async Task<ListNode> Deserialize(Stream inputStream)
        {
            List<ListNode> nodes = new List<ListNode>
            {
                new ListNode { Previous = null },
                new ListNode()
            };

            int counter = 0;
            try
            {
                while (true)
                {
                    string line;
                    try
                    {
                        line = await ReadStringAsync(inputStream);
                    }
                    catch (EndOfStreamException)
                    {
                        break;
                    }

                    int rand = await ReadIntAsync(inputStream);
                    nodes[counter].Data = line != string.Empty
                        ? line
                        : null;

                    int needNodesInArrayCount = int.Max(rand + 1, counter + 2);
                    if (nodes.Count < needNodesInArrayCount)
                    {
                        int startCount = nodes.Count;
                        for (int i = 0; i < needNodesInArrayCount - startCount; i++)
                        {
                            nodes.Add(new ListNode());
                        }
                    }

                    nodes[counter].Next = nodes[counter + 1];
                    nodes[counter + 1].Previous = nodes[counter];

                    if (rand != int.MinValue)
                        nodes[counter].Random = nodes[rand];

                    counter++;
                }
            }
            catch (EndOfStreamException)
            {
            }

            if (counter == 0)
                return null;

            if (nodes.Count >= 2)
                nodes[^2].Next = null;

            return nodes[0];
        }

        public static async ValueTask<int> ReadIntAsync(Stream stream)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(sizeof(int));
            try
            {
                await stream.ReadExactlyAsync(buffer.AsMemory(0, sizeof(int)));
                return BitConverter.ToInt32(buffer, 0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public static async ValueTask<string> ReadStringAsync(Stream stream)
        {
            int length = await ReadIntAsync(stream);

            if (length == 0)
                return string.Empty;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                await stream.ReadExactlyAsync(buffer.AsMemory(0, length));
                return Utf8.GetString(buffer, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
