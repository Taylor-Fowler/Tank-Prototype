using Network;
using UnityEngine;

public interface IManager
{
    NetworkService NetworkService { get; }

    void Startup(NetworkService networkService);
}
