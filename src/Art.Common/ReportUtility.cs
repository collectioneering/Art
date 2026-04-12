using System.Buffers;
using System.Security.Cryptography;

namespace Art.Common;

/// <summary>
/// Provides utilities for updating <see cref="IProgress{T}"/> of <see cref="float"/>.
/// </summary>
public static class ReportUtility
{
    private const int BufferSize = 512 * 1024; // prev 8192

    /// <summary>
    /// Copies a <see cref="Stream"/> to another, with reporting.
    /// </summary>
    /// <param name="sourceStream">Stream to copy from.</param>
    /// <param name="targetStream">Stream to copy to.</param>
    /// <param name="context">Report context.</param>
    /// <param name="refreshContext">Additional context to call <see cref="IRefreshContext.Refresh"/> on.</param>
    /// <param name="contentLength">Content length.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task CopyToWithReportAsync(
        Stream sourceStream,
        Stream targetStream,
        IProgress<float> context,
        IRefreshContext? refreshContext,
        long? contentLength,
        CancellationToken cancellationToken = default)
    {
        byte[] buf = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            var mem = buf.AsMemory(0, BufferSize);
            long total = 0;
            while (true)
            {
                int read;
                if ((read = await sourceStream.ReadAsync(mem, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    await targetStream.WriteAsync(mem[..read], cancellationToken).ConfigureAwait(false);
                    total += read;
                    context.Report(Math.Clamp((float)((double)total / contentLength ?? total), 0.0f, 1.0f));
                    refreshContext?.Refresh();
                }
                else
                {
                    context.Report(1.0f);
                    refreshContext?.Refresh();
                    break;
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }

    /// <summary>
    /// Computes hash by asynchronously reading a  <see cref="Stream"/>, with reporting.
    /// </summary>
    /// <param name="sourceStream">Stream to copy from.</param>
    /// <param name="hashAlgorithm">Hash algorithm.</param>
    /// <param name="context">Report context.</param>
    /// <param name="refreshContext">Additional context to call <see cref="IRefreshContext.Refresh"/> on.</param>
    /// <param name="contentLength">Content length.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async Task<byte[]> ComputeHashWithReportAsync(
        Stream sourceStream,
        HashAlgorithm hashAlgorithm,
        IProgress<float> context,
        IRefreshContext? refreshContext,
        long? contentLength, CancellationToken cancellationToken = default)
    {
        byte[] buf = ArrayPool<byte>.Shared.Rent(BufferSize);
        try
        {
            var mem = buf.AsMemory(0, BufferSize);
            long total = 0;
            while (true)
            {
                int read;
                if ((read = await sourceStream.ReadAsync(mem, cancellationToken).ConfigureAwait(false)) != 0)
                {
                    hashAlgorithm.TransformBlock(buf, 0, read, null, 0);
                    total += read;
                    context.Report(Math.Clamp((float)((double)total / contentLength ?? total), 0.0f, 1.0f));
                    refreshContext?.Refresh();
                }
                else
                {
                    context.Report(1.0f);
                    refreshContext?.Refresh();
                    break;
                }
            }
            hashAlgorithm.TransformBlock([], 0, 0, null, 0);
            hashAlgorithm.TransformFinalBlock([], 0, 0);
            return hashAlgorithm.Hash ?? throw new InvalidOperationException();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buf);
        }
    }
}
