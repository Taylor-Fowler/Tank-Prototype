using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPun
{
    public GameObject PlayerPrefab;

    private void Start()
    {
        PlayerController.LocalPlayer = PhotonNetwork.Instantiate(PlayerPrefab.name, Vector3.zero, PlayerPrefab.transform.rotation).GetComponent<PlayerController>();
    }

}
