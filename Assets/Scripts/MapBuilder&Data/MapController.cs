using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

    private MapData _data = new MapData();
    private int[,] _CurrentLevel;
    public Transform Floor;
    public Transform Wall1;
    public Transform PU01;
    public float PUUpValue = 0.2f;
    private Transform _FloorObjects;
    private Transform _WallObjects;
    private Transform _PowerUpObjects;


    // Use this for initialization
    void Start () {
        _FloorObjects = transform.Find("FloorObjects");
        _WallObjects = transform.Find("WallObjects");
        _PowerUpObjects = transform.Find("PowerUpObjects");
        // this can and wil be tweaked .... for dev purposes ... just getting 1 level
        _CurrentLevel = _data.GetLevelData(1);
        MakeLevel();
	}
	
    public void MakeLevel()
    {
        // Eliminate what is there already
        DestroyLevel();

        double Start = Time.realtimeSinceStartup;
        int first = _CurrentLevel.GetLength(0);
        int second = _CurrentLevel.GetLength(1);
        Debug.Log("Size [" + first.ToString() + "," + second.ToString() + "]");

        // Best add a floor
        Transform myFloor = (Transform)Instantiate(Floor, new Vector3(first / 2, 0, second / 2), Quaternion.identity);
        myFloor.transform.localScale = new Vector3(first / 10, 1, second / 10);
        myFloor.transform.parent = _FloorObjects;

        // Now add the 1's
        for (int i = 0; i < first ; i++)
        {
            for (int j = 0; j < second ;j++)
            {
                switch (_CurrentLevel[i,j])
                {
                    case 0:
                        break;
                    case 1:
                        Transform myWall = (Transform)Instantiate(Wall1, new Vector3(i, 0.5f, j), Quaternion.identity);
                        myWall.transform.parent = _WallObjects;
                        break;
                        // anything 11 - 19 = PowerUp Items
                    case 11:
                        Transform myPU = (Transform)Instantiate(PU01, new Vector3(i, PUUpValue, j), Quaternion.identity);
                        myPU.transform.parent = _PowerUpObjects;
                        break;
                    default:
                        break;
                }
            }
        }
        Debug.Log("Time to Destroy old and build " + first.ToString() + "x" + second.ToString() + " Level = " + (Time.realtimeSinceStartup - Start).ToString() + " secs");
    }

    public void DestroyLevel ()
    {
        foreach (Transform child in _FloorObjects) GameObject.Destroy(child.gameObject);
        foreach (Transform child in _WallObjects) GameObject.Destroy(child.gameObject);
        foreach (Transform child in _PowerUpObjects) GameObject.Destroy(child.gameObject);
    }


	// Update is called once per frame
	void Update () {
        // DEVELOPMENT PURPOSES ONLY
        if (Input.GetKeyDown("9")) DestroyLevel();
        if (Input.GetKeyDown("0")) MakeLevel();
	}
}
