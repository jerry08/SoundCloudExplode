﻿using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Utils.Extensions;

internal static class StreamExtensions
{
    public static async ValueTask CopyToAsync(
        this Stream source,
        Stream destination,
        long totalLength,
        IProgress<double>? progress = null,
        int bufferSize = 0x1000,
        CancellationToken cancellationToken = default
    )
    {
        using var buffer = MemoryPool<byte>.Shared.Rent(bufferSize);

        var totalBytesRead = 0L;
        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer.Memory, cancellationToken);
            if (bytesRead <= 0)
                break;

            await destination.WriteAsync(buffer.Memory[..bytesRead], cancellationToken);

            totalBytesRead += bytesRead;
            progress?.Report(1.0 * totalBytesRead / totalLength);
        }
    }
}
