using System.Text.Json;
using Microsoft.Toolkit.HighPerformance;
using Microsoft.Toolkit.HighPerformance.Buffers;

namespace Concorde.Extensions;

public static class JsonExtensions
{
    public static async Task<Memory<byte>> SerializeAsync<TValue>(
        this TValue value,
        JsonSerializerOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var memoryOwner = MemoryOwner<byte>.Allocate(4096);
        var stream = memoryOwner.AsStream();
        
        await JsonSerializer.SerializeAsync(stream, value, options, cancellationToken);

        return memoryOwner.Memory.TrimEnd<byte>(0);
    }
}