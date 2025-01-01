using System.Collections.Generic;
using WakeOnLanLibrary.Application.Models;

namespace WakeOnLanLibrary.Infrastructure.Builders
{
    public interface IScriptBuilder
    {
        string BuildScript(IEnumerable<WakeOnLanRequest> requests);
    }

}
