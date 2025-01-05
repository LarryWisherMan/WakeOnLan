using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class RequestScheduler : IRequestScheduler
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentQueue<Func<Task>> _queue = new();

        public RequestScheduler(int maxConcurrency)
        {
            if (maxConcurrency <= 0)
                throw new ArgumentException("Max concurrency must be greater than zero.", nameof(maxConcurrency));

            _semaphore = new SemaphoreSlim(maxConcurrency);
        }

        public void Schedule(Func<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task), "Task cannot be null.");

            _queue.Enqueue(task);
        }

        public async Task ExecuteScheduledTasksAsync(CancellationToken cancellationToken = default)
        {
            var runningTasks = new List<Task>();

            while (_queue.TryDequeue(out var task))
            {
                await _semaphore.WaitAsync(cancellationToken);

                // Start task and add to running tasks
                var runningTask = Task.Run(async () =>
                {
                    try
                    {
                        await task();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                }, cancellationToken);

                runningTasks.Add(runningTask);
            }

            // Wait for all running tasks to complete
            await Task.WhenAll(runningTasks);
        }

        public async Task ClearScheduledTasksAsync(CancellationToken cancellationToken = default)
        {
            // Drain the queue
            while (_queue.TryDequeue(out _))
            {
                await Task.Yield(); // Yield to ensure smooth cancellation if requested
            }
        }
    }
}
