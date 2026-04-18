using System.Collections.Concurrent;

namespace Art.Common.Async;

internal class EagerAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IAsyncEnumerator<T> _e;
    private readonly CancellationToken _cancellationToken;
    private readonly ConcurrentQueue<JobState> _qt;
    private readonly ConcurrentQueue<T> _queue;
    internal Action<EnumeratorResult<T>>? _invokeTarget { get; init; }
    private readonly int _maxPreemptiveAccesses;
    private volatile bool _over;

    public EagerAsyncEnumerator(IAsyncEnumerator<T> e, int maxPreemptiveAccesses = -1, CancellationToken cancellationToken = default)
    {
        if (maxPreemptiveAccesses is not (-1 or > 0))
        {
            throw new ArgumentOutOfRangeException(nameof(maxPreemptiveAccesses));
        }
        _e = e;
        _cancellationToken = cancellationToken;
        _qt = new ConcurrentQueue<JobState>();
        _queue = new ConcurrentQueue<T>();
        _maxPreemptiveAccesses = maxPreemptiveAccesses;
        Current = default!;
        ScheduleNext();
    }

    private class JobState
    {
        public volatile bool StoppedByAccessLimit;
        public volatile bool CallFlag;
        public Task<EnumeratorResult<T>> Task = null!;
    }

    private void ScheduleNext()
    {
        JobState state;
        do
        {
            state = new JobState();
            _qt.Enqueue(state);
            state.Task = NextInternal(state);
            state.CallFlag = true;
            if (_invokeTarget != null)
            {
                state.Task.ContinueWith(v => _invokeTarget(v.Result), _cancellationToken);
            }
        } while (state.Task.IsCompletedSuccessfully && state.Task.Result.Valid && state.CallFlag && !state.StoppedByAccessLimit);
    }

    private async Task<EnumeratorResult<T>> NextInternal(JobState jobState)
    {
        bool advanced = await _e.MoveNextAsync().ConfigureAwait(false);
        _cancellationToken.ThrowIfCancellationRequested();
        T? value;
        if (advanced)
        {
            value = _e.Current;
            _queue.Enqueue(_e.Current);

            if (_maxPreemptiveAccesses != -1 && _qt.Count >= _maxPreemptiveAccesses)
            {
                jobState.StoppedByAccessLimit = true;
                jobState.CallFlag = true;
            }
            else
            {
                jobState.StoppedByAccessLimit = false;
                if (jobState.CallFlag)
                {
                    jobState.CallFlag = false;
                    ScheduleNext();
                }
            }
        }
        else
        {
            value = default;
            jobState.CallFlag = false;
            jobState.StoppedByAccessLimit = false;
        }
        return new EnumeratorResult<T>(value, advanced);
    }

    public ValueTask DisposeAsync() => _e.DisposeAsync();

    public async ValueTask<bool> MoveNextAsync()
    {
        if (_over)
        {
            return false;
        }
        if (!_qt.TryDequeue(out JobState? state))
        {
            throw new InvalidOperationException($"State violation ({nameof(_qt)}");
        }
        (_, bool advanced) = await state.Task.ConfigureAwait(false);
        if (advanced)
        {
            if (!_queue.TryDequeue(out T? value))
            {
                throw new InvalidOperationException($"State violation ({nameof(_queue)})");
            }
            Current = value;
            if (state.CallFlag)
            {
                ScheduleNext();
            }
        }
        else
        {
            _over = true;
            Current = default!;
        }
        return advanced;
    }

    public T Current { get; private set; }
}

internal record struct EnumeratorResult<T>(T? Value, bool Valid);
