using System.Collections.Generic;
using System.Management.Automation.Runspaces;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Application.Interfaces
{
    public interface IRequestGrouper
    {
        Dictionary<Runspace, List<WakeOnLanRequest>> GroupByRunspace(IEnumerable<WakeOnLanRequest> requests);
    }

}
