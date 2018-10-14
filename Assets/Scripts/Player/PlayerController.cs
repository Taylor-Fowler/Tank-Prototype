using UnityEngine;
using UnityEngine.Networking;
using Photon.Pun;

public class PlayerController : MonoBehaviourPun
{
    public static PlayerController LocalPlayer;

    [SerializeField]
    private GameObject CameraPrefab;
    [SerializeField]
    private GameObject TurretObject;

    private void Awake()
    {
        if(photonView.IsMine)
        {
            LocalPlayer = this;
            Instantiate(CameraPrefab, transform);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        float forward = 0;
        float right = 0;

        if (Input.GetKey(KeyCode.D))
        {
            right += 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            right -= 1;
        }

        if (Input.GetKey(KeyCode.W))
        {
            forward += 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            forward -= 1;
        }

        transform.Translate(forward * Time.deltaTime * Vector3.forward);
        transform.Rotate(right * Vector3.up);

        var mousePosition = Input.mousePosition;
        var tankPosition = Camera.main.WorldToScreenPoint(transform.position);
        var forwardVector = mousePosition - tankPosition;

        forwardVector.z = forwardVector.y;
        forwardVector.y = 0f;
        forwardVector.Normalize();

        TurretObject.transform.localRotation = Quaternion.LookRotation(forwardVector);
    }
}
