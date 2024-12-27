using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Services;

public class PersistentWolService : WakeOnLanService
{
    private static readonly WakeOnLanResultCache _persistentResultCache = new();
    private static readonly MonitorCache _persistentMonitorCache = new();

    public PersistentWolService(
        IMagicPacketSender packetSender,
        IComputerValidator computerValidator,
        IComputerFactory computerFactory,
        IRunspaceManager runspaceManager)
        : base(
            packetSender,
            computerValidator,
            computerFactory,
            runspaceManager,
            _persistentResultCache,
            new MonitorService(_persistentMonitorCache)) // Pass the persistent cache here
    {
    }

    public WakeOnLanResultCache ResultCache => _persistentResultCache;
    public MonitorCache MonitorCache => _persistentMonitorCache;

}
