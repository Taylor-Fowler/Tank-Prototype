using UnityEngine;

using Photon.Pun;

public class ViewRoom : MonoBehaviourPunCallbacks
{
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
