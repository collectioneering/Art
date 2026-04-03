using Art.Common.Async;

namespace Art.Common.Tests;

public class EagerTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UnlimitedEager_CompletesFully(bool syncCounter)
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        SharedValue sharedValue = new() { Value = 5 };
        var v = (syncCounter ? CounterSync(5, 5) : CounterAsync(5, 5, TimeSpan.FromMilliseconds(30)))
            .EagerTestAsync(invokeTarget: v =>
            {
                lock (sharedValue)
                {
                    int newValue = v.Valid ? v.Value : 10;
                    sharedValue.Value = Math.Max(sharedValue.Value, newValue);
                }
            });
        var v2 = v.GetAsyncEnumerator(testCancellationToken);
        if (!syncCounter)
        {
            await CompleteBatch(9, sharedValue, TimeSpan.FromSeconds(10));
        }
        AssertCounterSectionCompletedSynchronously(v2, 5, 5);
        if (!syncCounter)
        {
            await CompleteBatch(10, sharedValue, TimeSpan.FromSeconds(10));
        }
        Task<bool> tE = v2.MoveNextAsync().AsTask();
        Assert.True(tE.IsCompleted);
        Assert.True(tE.IsCompletedSuccessfully);
        Assert.False(await tE);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task LimitedEager_PartiallyCompletes(bool syncCounter)
    {
        var testCancellationToken = TestContext.Current.CancellationToken;
        const int initialLimit = 5;
        SharedValue sharedValue = new() { Value = 5 };
        var v = (syncCounter ? CounterSync(5, 15) : CounterAsync(5, 15, TimeSpan.FromMilliseconds(30)))
            .EagerTestAsync(initialLimit, v =>
            {
                lock (sharedValue)
                {
                    int newValue = v.Valid ? v.Value : 20;
                    sharedValue.Value = Math.Max(sharedValue.Value, newValue);
                }
            });
        var v2 = v.GetAsyncEnumerator(testCancellationToken);
        int i = 5, max = 5 + 15;
        while (i < max)
        {
            int iterationCount = Math.Min(initialLimit, max - i);
            if (!syncCounter)
            {
                await CompleteBatch(i + iterationCount - 1, sharedValue, TimeSpan.FromSeconds(10));
            }
            AssertCounterSectionCompletedSynchronously(v2, i, iterationCount);
            i += iterationCount;
            if (i == max)
            {
                break;
            }
        }
        if (!syncCounter)
        {
            await CompleteBatch(20, sharedValue, TimeSpan.FromSeconds(10));
        }
        Task<bool> tE = v2.MoveNextAsync().AsTask();
        Assert.True(tE.IsCompleted);
        Assert.True(tE.IsCompletedSuccessfully);
        Assert.False(await tE);
    }

    private static async Task CompleteBatch(int waitValue, SharedValue sharedValue, TimeSpan cancellationDuration)
    {
        using CancellationTokenSource cts = new(cancellationDuration);
        while (sharedValue.Value < waitValue)
        {
            await Task.Yield();
            cts.Token.ThrowIfCancellationRequested();
        }
        await Task.Yield();
    }

    private class SharedValue
    {
        public volatile int Value;
    }

    private static void AssertCounterSectionCompletedSynchronously(IAsyncEnumerator<int> v2, int start, int count)
    {
        for (int i = 0; i < count; i++)
        {
            Task<bool> t = v2.MoveNextAsync().AsTask();
            Assert.True(t.IsCompleted);
            Assert.True(t.IsCompletedSuccessfully);
            Assert.True(t.Result);
            Assert.Equal(start + i, v2.Current);
        }
    }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    private static async IAsyncEnumerable<int> CounterSync(int start, int count)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        foreach (int value in Enumerable.Range(start, count))
        {
            yield return value;
        }
    }

    private static async IAsyncEnumerable<int> CounterAsync(int start, int count, TimeSpan splitTimeSpan)
    {
        foreach (int value in Enumerable.Range(start, count))
        {
            await Task.Delay(splitTimeSpan).ConfigureAwait(false);
            yield return value;
        }
    }
}

internal static class EagerAsyncTestExtensions
{
    public static IAsyncEnumerable<T> EagerTestAsync<T>(this IAsyncEnumerable<T> asyncEnumerable, int maxPreemptiveAccesses = -1, Action<EnumeratorResult<T>>? invokeTarget = null)
    {
        return new EagerAsyncEnumerable<T>(asyncEnumerable, maxPreemptiveAccesses) { _invokeTarget = invokeTarget };
    }
}
