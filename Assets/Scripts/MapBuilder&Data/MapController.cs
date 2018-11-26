///////////////////////////////////////////////////////////////////////////
// CI6510 Optimised Game Programming    (Kingston University)            //
// Course Work 1 : Network Game with Plug-ins                            //
// Submission by Max Bryans (K1628007) and Taylor Fowler (K1612040)      //
// December 2018                                                         //
///////////////////////////////////////////////////////////////////////////
using UnityEngine;

public class MapController : MonoBehaviour
{
    #region INSPECTOR REQUIRED SETTABLES
    public int MapDataToUse = 1; // default Map For 2
    public Transform Floor;
    public Transform SpawnPoint;
    public Transform Wall1; // Interior Wall block
    public Transform Wall2; // Interior Column
    public Transform Wall3; // Low Wall
    public Transform Wall4; // Map Edge Wall
    public Transform PUHealth;
    public Transform PUMove;
    public Transform PUFire;
    public float PUUpValue = 0.2f; // how high Power Up items spawn above the floor
    #endregion

    #region PRIVATE VARIABLES
    private MapData _data = new MapData();
    private int[,] _CurrentLevel;
    private Transform _FloorObjects;
    private Transform _WallObjects;
    private Transform _PowerUpObjects;
    private Transform _SpawnPoints;
    private Transform _InitialSpawnPoints;
    #endregion


    #region UNITY API
    private void Start ()
    {
        _FloorObjects = transform.Find("FloorObjects");
        _WallObjects = transform.Find("WallObjects");
        _PowerUpObjects = transform.Find("PowerUpObjects");
        _SpawnPoints = transform.Find("SpawnPoints");
        _InitialSpawnPoints = transform.Find("InitialSpawns");
        // Dev note:  1 = Map for 2, 2 = Map for 4
        _CurrentLevel = _data.GetLevelData(MapDataToUse);
        MakeLevel();
	}
    #endregion

    #region PUBLIC METHODS
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
                    case 1: // basic cube wall
                        Transform myWall1 = (Transform)Instantiate(Wall1, new Vector3(i, 0.5f, j), Quaternion.identity);
                        myWall1.transform.parent = _WallObjects;
                        break;
                    case 2: // Cylinder wall end
                        Transform myWall2 = (Transform)Instantiate(Wall2, new Vector3(i, 0.6f, j), Quaternion.identity);
                        myWall2.transform.parent = _WallObjects;
                        break;
                    case 3: // Horizontal Low Wall
                        Transform myWall3 = (Transform)Instantiate(Wall3, new Vector3(i, 0.1f, j), Quaternion.identity);
                        myWall3.transform.parent = _WallObjects;
                        break;
                    case 4: // Vertical Low Wall
                        Transform myWall4 = (Transform)Instantiate(Wall3, new Vector3(i, 0.1f, j), Quaternion.identity);
                        myWall4.transform.Rotate(Vector3.up * 90);
                        myWall4.transform.parent = _WallObjects;
                        break;
                    case 8: // outer Wall
                        Transform myWall8 = (Transform)Instantiate(Wall4, new Vector3(i, 0.5f, j), Quaternion.identity);
                        myWall8.transform.parent = _WallObjects;
                        break;
                    // anything 11 - 19 = PowerUp Items
                    case 11:
                        Transform myPU1 = (Transform)Instantiate(PUHealth, new Vector3(i, PUUpValue, j), Quaternion.identity);
                        myPU1.transform.parent = _PowerUpObjects;
                        break;
                    case 12:
                        Transform myPU2 = (Transform)Instantiate(PUFire, new Vector3(i, PUUpValue, j), Quaternion.identity);
                        myPU2.transform.parent = _PowerUpObjects;
                        break;
                    case 13:
                        Transform myPU3 = (Transform)Instantiate(PUMove, new Vector3(i, PUUpValue, j), Quaternion.identity);
                        myPU3.transform.parent = _PowerUpObjects;
                        break;
                    // 90 = INITIAL SPAWN POINT
                    case 90:
                        Transform myISP = (Transform)Instantiate(SpawnPoint, new Vector3(i, 0.21f, j), Quaternion.identity); // MAGIC NUMBER ALERT ... 0.21f = ideal height for Tank to Spawn
                        myISP.transform.parent = _InitialSpawnPoints;
                        break;
                    // 99 = SPAWN POINT
                    case 99:
                        Transform mySP = (Transform)Instantiate(SpawnPoint, new Vector3(i, 0.21f, j), Quaternion.identity); // MAGIC NUMBER ALERT ... 0.21f = ideal height for Tank to Spawn
                        mySP.transform.parent = _SpawnPoints;
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
        foreach (Transform child in _SpawnPoints) GameObject.Destroy(child.gameObject);
        foreach (Transform child in _InitialSpawnPoints) GameObject.Destroy(child.gameObject);
    }

    public Transform[] ReportSpawns()
    {
        int x = _SpawnPoints.childCount;
        if (x == 0) return null; // safety net
        Transform[] report = new Transform[x];
        for (int i = 0; i < x; i++)
        {
            report[i] = _SpawnPoints.GetChild(i);
        }
        return report;
    }

    public Transform[] ReportInitialSpawns()
    {
        int x = _InitialSpawnPoints.childCount;
        if (x == 0) return null; // safety net
        Transform[] report = new Transform[x];
        for (int i = 0; i < x; i++)
        {
            report[i] = _InitialSpawnPoints.GetChild(i);
        }
        return report;
    }

    public Transform GetSpawn(InGameVariables playerVariable)
    {
        Transform closest = null;
        int length = _SpawnPoints.childCount;
        int longestDistanceOfPlayerClosestToSpawn = int.MinValue;

        for(int i = 0; i < length; i++)
        {
            Transform currentSpawn = _SpawnPoints.GetChild(i);
            int closestPlayer = int.MaxValue;

            foreach(var player in PlayerController.PlayerControllers)
            {
                if(player == playerVariable)
                {
                    continue;
                }

                Transform playerPosition = player.Controller.Position();
                if(playerPosition != null)
                {
                    int distance = (int)Vector3.Distance(playerPosition.position, currentSpawn.position);

                    if (distance < closestPlayer)
                    {
                        closestPlayer = distance;
                    }
                }

            }

            if(closestPlayer > longestDistanceOfPlayerClosestToSpawn)
            {
                longestDistanceOfPlayerClosestToSpawn = closestPlayer;
                closest = currentSpawn;
            }
        }
        
        return closest;
    }
    #endregion
}
