using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour {

    private MapData _data = new MapData();
    private int[,] _CurrentLevel;
    public Transform Floor;
    public Transform Wall1;

	// Use this for initialization
	void Start () {
        _CurrentLevel = _data.GetLevelData(1);
        MakeLevel();
	}
	
    public void MakeLevel()
    {
        double Start = Time.realtimeSinceStartup;
        int first = _CurrentLevel.GetLength(0);
        int second = _CurrentLevel.GetLength(1);
        Debug.Log("Size [" + first.ToString() + "," + second.ToString() + "]");

        // Best add a floor
        Transform myFloor = (Transform)Instantiate(Floor, new Vector3(first / 2, 0, second / 2), Quaternion.identity);
        myFloor.transform.localScale = new Vector3(first / 10, 1, second / 10);

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
                        Instantiate(Wall1, new Vector3(i, 0.5f, j), Quaternion.identity);
                        break;
                    default:
                        break;
                }
            }
        }
        Debug.Log("Time to build " + first.ToString() + "x" + second.ToString() + " Level = " + (Time.realtimeSinceStartup - Start).ToString() + " secs");
    }


	// Update is called once per frame
	void Update () {
		
	}
}
