using System;
using System.Threading.Tasks;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface ITaskRunner
    {
        void Run(Func<Task> task);
    }

}
