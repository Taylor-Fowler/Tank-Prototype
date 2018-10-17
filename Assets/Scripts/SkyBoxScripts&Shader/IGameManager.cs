using UnityEngine;
using System.Collections;

public interface IGameManager
{

    ManagerStatus status { get; }
    void StartUp(NetService service);
}
