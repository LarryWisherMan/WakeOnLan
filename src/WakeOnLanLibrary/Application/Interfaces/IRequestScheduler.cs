using System;
using System.Threading;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{


    public interface IRequestScheduler
    {
        void Schedule(Func<Task> task);
        Task ExecuteScheduledTasksAsync(CancellationToken cancellationToken = default);
        Task ClearScheduledTasksAsync(CancellationToken cancellationToken = default);
    }


}