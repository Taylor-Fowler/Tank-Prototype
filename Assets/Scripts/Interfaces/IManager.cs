using System;
using Network;

public interface IManager
{
    NetworkService NetworkService { get; }
    ManagerStatus Status { get; }

    void Startup(NetworkService networkService);
}
