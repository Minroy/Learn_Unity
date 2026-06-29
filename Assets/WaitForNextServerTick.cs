using PurrNet;
using PurrNet.Modules;
using UnityEngine;

/// <summary>
/// Wait for the next Tick to fire;
/// </summary>
public class WaitForNextServerTick : CustomYieldInstruction
{
    public override bool keepWaiting => !_tickPassed;
    private bool _tickPassed = false;
    private TickManager tm;

    public WaitForNextServerTick(bool asServer)
    {
        tm = InstanceHandler.NetworkManager.GetModule<TickManager>(asServer);
        tm.onReliablePostTick += OnTick;
    }

    private void OnTick()
    {
        _tickPassed = true;
        if (_tickPassed)
            tm.onReliablePostTick -= OnTick;
    }

    ~WaitForNextServerTick()
    {
        tm.onReliablePostTick -= OnTick;
    }
}

/// <summary>
/// Wait for set amounts of ticks to pass
/// </summary>
public class WaitForSetTicks : CustomYieldInstruction
{
    public override bool keepWaiting
    {
        get
        {
            return _tickPassed > 0;
        }
    }
    private int _tickPassed;
    private TickManager tm;
    public WaitForSetTicks(int time, bool asServer)
    {
        _tickPassed = time;
        tm = InstanceHandler.NetworkManager.GetModule<TickManager>(asServer);
        tm.onReliableTick += OnTick;
    }

    private void OnTick()
    {
        _tickPassed -= 1;
        if (_tickPassed <= 0)
            tm.onReliableTick -= OnTick;
    }

    ~WaitForSetTicks()
    {
        tm.onReliableTick -= OnTick;
    }
}
