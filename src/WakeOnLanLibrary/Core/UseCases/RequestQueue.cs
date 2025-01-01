using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanLibrary.Core.UseCases
{
    public class RequestQueue : IRequestQueue
    {
        private readonly SemaphoreSlim _semaphore;
        private ConcurrentQueue<Func<Task>> _queue = new();

        public RequestQueue(int maxConcurrency)
        {
            if (maxConcurrency <= 0)
                throw new ArgumentException("Max concurrency must be greater than zero.", nameof(maxConcurrency));

            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public void Enqueue(Func<Task> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request), "Request cannot be null.");

            _queue.Enqueue(request);
        }

        public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
        {
            while (_queue.TryDequeue(out var request))
            {
                await _semaphore.WaitAsync(cancellationToken);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await request();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, cancellationToken);
            }
        }

        public async Task ClearAsync(CancellationToken cancellationToken = default)
        {
            // Drain the queue
            while (_queue.TryDequeue(out _))
            {
                await Task.Yield(); // Yield to ensure smooth cancellation if requested
            }
        }
    }
}
