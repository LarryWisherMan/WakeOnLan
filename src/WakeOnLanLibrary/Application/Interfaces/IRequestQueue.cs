using System;
using System.Threading;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{


    public interface IRequestQueue
    {
        void Enqueue(Func<Task> request);
        Task ProcessQueueAsync(CancellationToken cancellationToken = default);
        Task ClearAsync(CancellationToken cancellationToken = default); // Add this method
    }

}