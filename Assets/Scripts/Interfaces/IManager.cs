using Network;
using UnityEngine;

public interface IManager
{
    NetworkService NetworkService { get; }
    ManagerStatus Status { get; }

    void Startup(NetworkService networkService);
}
