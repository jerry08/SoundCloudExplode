using System;
using System.Threading;
using System.Threading.Tasks;

namespace SoundCloudExplode.Utils.Extensions;

internal static class GenericExtensions
{
    public static TOut Pipe<TIn, TOut>(this TIn input, Func<TIn, TOut> transform) => transform(input);

    public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken, Action action, bool useSynchronizationContext = true)
    {
        using (cancellationToken.Register(action, useSynchronizationContext))
        {
            try
            {
                return await task;
            }
            catch (Exception ex)
            {

                if (cancellationToken.IsCancellationRequested)
                {
                    // the Exception will be available as Exception.InnerException
                    throw new OperationCanceledException(ex.Message, ex, cancellationToken);
                }

                // cancellation hasn't been requested, rethrow the original Exception
                throw;
            }
        }
    }
}