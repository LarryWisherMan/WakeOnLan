using System;
using System.Threading.Tasks;
using WakeOnLanLibrary.Application.Interfaces;

namespace WakeOnLanLibrary.Infrastructure.Services
{
    public class TaskRunner : ITaskRunner
    {
        public void Run(Func<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            Task.Run(task);
        }
    }

}
