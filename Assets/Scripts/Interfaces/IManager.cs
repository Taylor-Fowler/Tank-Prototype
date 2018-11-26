///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using System;
using Network;

public interface IManager
{
    NetworkService NetworkService { get; }
    ManagerStatus Status { get; }

    void Startup(NetworkService networkService);
    void Restart();
    void Shutdown();
}
