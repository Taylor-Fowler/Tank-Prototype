using UnityEngine;

public class GameController : MonoBehaviour {

    // TBA variables placeholders
    [Header("Necessary Tranforms, Cameras and Objects")]
    public Camera PlayerCamera;

    [Header("Master Game Variables")]
    public int PlayerCount = 0;

    [Header("Serialized Fields - for debug reference only")]
    [SerializeField]  string DeviceID = null;
    [SerializeField]  CommsManager Comms;

    // --------------------//
    // establish Singelton //
    // ------------------- //
    public static GameController Instance
    {
        get
        {
            return instance;
        }
    }
    private static GameController instance = null;
    void Awake()
    {
        if (instance)
        {
            Debug.Log("Already a GameController running - going to die now .....");
            DestroyImmediate(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    //---------------------------//
    // Finished Singelton set up //
    // --------------------------//



    // Doesn't do much at the mo ... probably will configure via a Plug-in eventually
    void Start () {
        // Get Unique Device ID N.B. was attached to a "if null" condition .... fell over on the second run (I'm guessing it was internally saves as something)
        DeviceID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("DevID: " + DeviceID + " Length : " + DeviceID.Length + " Chars");

        // add Comms
        Comms = gameObject.AddComponent<CommsManager>();

	}
	
	// Update is called once per frame // currently masked to save a bit of update overhead
	//void Update () {
	//	
	//}
}
