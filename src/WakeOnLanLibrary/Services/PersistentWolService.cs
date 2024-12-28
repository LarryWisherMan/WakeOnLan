using System;
using WakeOnLanLibrary.Interfaces;
using WakeOnLanLibrary.Services;

public class PersistentWolService : WakeOnLanService
{
    private static readonly WakeOnLanResultCache _persistentResultCache = new();
    private static readonly MonitorCache _persistentMonitorCache = new();
    private readonly IRunspaceManager _runspaceManager;

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
        _runspaceManager = runspaceManager ?? throw new ArgumentNullException(nameof(runspaceManager));
    }

    public WakeOnLanResultCache ResultCache => _persistentResultCache;
    public MonitorCache MonitorCache => _persistentMonitorCache;

    public void ClearRunspaces()
    {
        try
        {
            _runspaceManager.CloseAllRunspaces();
        }
        catch (Exception ex)
        {
            // Log or handle the exception as appropriate
            Console.WriteLine($"Failed to clear runspaces: {ex.Message}");
        }
    }
}
