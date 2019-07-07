using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class ShipComponentState
{
    [SerializeField] private ShipComponentsEnum component;
    [SerializeField] private bool collected;

    public ShipComponentsEnum Component { get => component; set => component = value; }
    public bool Collected { get => collected; set => collected = value; }

    public ShipComponentState(ShipComponentsEnum e, bool b)
    {
        component = e;
        collected = b;
    }
}

public class WorldController : MonoBehaviour
{
    //Fields-----------------------------------------------------------------------------------------------------------------------------------------

    //Serialized Fields
    [Header("World Spawning Rules")]
    [SerializeField] private int width = 31;
    [SerializeField] private int length = 31;
    [SerializeField] private bool spawnResources = false, gameWin = false, gameOver = false;
    [SerializeField] int mineralSpawnChance = 5, fuelSpawnChance = 5, powerSpawnChance = 5, organSpawnChance = 5;

    [Header("Prefab/Gameobject assignment")]
    [SerializeField] private GameObject ground;

    [SerializeField] GameObject planeGridprefab, hubPrefab, mineralPrefab, fuelPrefab, powerPrefab, organPrefab, tilePrefab;

    [SerializeField] private Hub hub = null;
    [SerializeField] private TileData[,] tiles;
    [SerializeField] private List<ShipComponentState> shipComponents = new List<ShipComponentState>();
    [SerializeField] protected GameObject serialCamera;

    [Header("Public variable?")]
    public bool InBuildMode;
    public GameObject pause;

    //Non-Serialized Fields
    private GameObject temp, PlaneSpawn, TowerSpawn, TowerToSpawn, tiletest, tmp;
    private GameObject[] objs;
    private TowerManager tm;
    private Vector3 pos;

    //Other Controllers
    private ResourceController resourceController;
    private UIController uiController;
    private CameraController cameraController;

    private List<TileData> ThrusterList = new List<TileData>();

    private List<TileData> activeTiles = new List<TileData>();
    public List<TileData> ActiveTiles { get => activeTiles; }

    private List<TileData> disableTiles = new List<TileData>();
    public List<TileData> DisableTiles { get => disableTiles; }

    //Cursor Locking to centre
    private CursorLockMode wantedMode;

    //Flags
    private bool hubBuilt = false;
    private bool hubDestroyed = false;

    private int index;
    private bool thrusterToggle = true;
    public PowerSource tempPower;

    //Public Properties
    // public static WorldController used to get the instance of the WorldManager from anywhere.
    public static WorldController Instance { get; protected set; }
    public TileData[,] Tiles { get => tiles; }
    public GameObject Ground { get => ground; set => ground = value; }
    public int Width { get => width; }
    public int Length { get => length; }
    public Hub Hub { get => hub; set => hub = value; }
    public bool HubDestroyed { get => hubDestroyed; set => hubDestroyed = value; }
    public List<ShipComponentState> ShipComponents { get => shipComponents; }
    public bool GameWin { get => gameWin; set => gameWin = value; }
    public bool GameOver { get => gameOver; set => gameOver = value; }

    //Start-Up Methods-------------------------------------------------------------------------------------------------------------------------------
    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        InBuildMode = false;
        Instance = this;
        SetPause(false);

        tm = FindObjectOfType<TowerManager>();

        Cursor.lockState = wantedMode;
        Cursor.visible = (CursorLockMode.Locked != wantedMode);

        serialCamera = GameObject.Find("CameraTarget");
        cameraController = GameObject.Find("CameraTarget").GetComponent<CameraController>();
        uiController = GetComponent<UIController>();
        resourceController = ResourceController.Instance;
    }

    private void Start()
    {
        index = 0;
        InstantiateTileArray();
        ConnectAdjacentTiles();
        SetResourcesToTiles();
        SetBuildingsToTiles();
        SetLandmarksToTiles();
        TutorialController.Instance.StartTutorial();
    }

    void SetResourcesToTiles()
    // Collect all Resources in the scene and assign them to the closest tile
    {
        ResourceNode[] resources = FindObjectsOfType<ResourceNode>();

        foreach (ResourceNode resourceNode in resources)
        {
            TileData tile = GetTileAt(resourceNode.transform.position);
            tile.Resource = resourceNode;
            resourceNode.Location = tile;

            resourceNode.MaxHealth = 200;
            resourceNode.Health = resourceNode.MaxHealth;

            // Centre on tile
            Vector3 pos = resourceNode.transform.position;
            pos.x = Mathf.Round(pos.x);
            pos.z = Mathf.Round(pos.z);
            resourceNode.transform.position = pos;
        }
    }

    void SetBuildingsToTiles()
    // Collect all Buildings in the scene and assign them to the closest tile
    {
        Building[] buildings = FindObjectsOfType<Building>();
        ShipComponent[] shipComponentList = FindObjectsOfType<ShipComponent>();

        foreach (Building b in buildings)
        {
            if (b.BuildingType == BuildingType.Hub)
            {
                TileData tile = GetTileAt(b.transform.position);
                tile.Building = b;
                b.Location = tile;
                b.Animator = b.GetComponentInChildren<Animator>();
                b.Animator.SetBool("Built", true);

                // Centre on tile
                Vector3 pos = b.transform.position;
                pos.x = Mathf.Round(pos.x);
                pos.z = Mathf.Round(pos.z);
                b.transform.position = pos;

                b.Place();
                break;
            }
        }

        foreach (Building b in buildings)
        {
            if (b.BuildingType != BuildingType.Hub)
            {
                TileData tile = GetTileAt(b.transform.parent.position);
                b.Location = tile;

                b.Animator = b.GetComponentInChildren<Animator>();
                b.Animator.SetBool("Built", true);
                b.Place();
            }
        }

        foreach (ShipComponent s in shipComponentList)
        {
            TileData tile = GetTileAt(s.transform.position);
            s.Location = tile;
            ShipComponents.Add(new ShipComponentState(s.Id, false));
            s.gameObject.SetActive(false);
        }
    }

    void SetLandmarksToTiles()
    {
        Landmark[] landmarks = FindObjectsOfType<Landmark>();

        foreach (Landmark l in landmarks)
        {
            l.Location = GetTileAt(l.transform.position);
        }
    }

    private void InstantiateTileArray()
    {
        tiles = new TileData[width, length];
        GameObject quad = GameObject.Find("Quads");
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                TileData tile = new TileData(x, z);
                tiles[x, z] = tile;
                pos = new Vector3(tile.X, 0, tile.Z);

                GameObject tileObject = Instantiate(tilePrefab);
                tileObject.transform.position = new Vector3(tile.X, 0.1f, tile.Z);
                tileObject.transform.SetParent(quad.transform);

                if (spawnResources)
                {
                    if (UnityEngine.Random.Range(1, 100) < mineralSpawnChance)
                    {
                        pos.y += 0.2f;
                        GameObject mineral = Instantiate(mineralPrefab, pos, Quaternion.Euler(0f, 0f, 0f));
                        tile.Resource = mineral.GetComponentInChildren<ResourceNode>();
                        mineral.GetComponentInChildren<ResourceNode>().Location = tile;
                        mineral.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < fuelSpawnChance)
                    {
                        pos.y += 0.3f;
                        GameObject fuel = Instantiate(fuelPrefab, pos, Quaternion.Euler(0, 0, 0));
                        tile.Resource = fuel.GetComponentInChildren<ResourceNode>();
                        fuel.GetComponentInChildren<ResourceNode>().Location = tile;
                        fuel.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < organSpawnChance)
                    {
                        pos.y += 0.3f;
                        GameObject organ = Instantiate(organPrefab, pos, Quaternion.Euler(0f, 180f, 0f));
                        tile.Resource = organ.GetComponentInChildren<ResourceNode>();
                        organ.GetComponentInChildren<ResourceNode>().Location = tile;
                        organ.transform.SetParent(ground.transform, true);
                    }
                    else if (UnityEngine.Random.Range(1, 100) < powerSpawnChance)
                    {
                        pos.y += 0.2f;
                        GameObject power = Instantiate(powerPrefab, pos, Quaternion.Euler(0f, 180f, 0f));
                        tile.Resource = power.GetComponentInChildren<ResourceNode>();
                        power.GetComponentInChildren<ResourceNode>().Location = tile;
                        power.transform.SetParent(ground.transform, true);
                    }
                }
            }
        }
    }

    //Connects each tile to its orthogonally adjacent and diagonally adjacent neighbours
    public void ConnectAdjacentTiles()
    {
        TileData a;

        if (gameObject.GetComponent<Fog>().ExpansionDirection == FogExpansionDirection.Orthogonal)
        {
            foreach (TileData t in tiles)
            {
                Vector2[] pts = { new Vector2(t.X, t.Z - 1), new Vector2(t.X, t.Z + 1), new Vector2(t.X - 1, t.Z), new Vector2(t.X + 1, t.Z) };

                foreach (Vector2 p in pts)
                {
                    if (p.x >= 0 && p.x < width && p.y >= 0 && p.y < length)
                    {
                        a = GetTileAt(p);

                        if (!t.AdjacentTiles.Contains(a))
                        {
                            t.AdjacentTiles.Add(a);
                            continue;
                        }
                    }
                }
            }
        }
        else
        {
            foreach (TileData t in tiles)
            {
                for (int i = t.X - 1; i <= t.X + 1; i++)
                {
                    if (i >= 0 && i < width)
                    {
                        for (int j = t.Z - 1; j <= t.Z + 1; j++)
                        {
                            if (j >= 0 && j < length)
                            {
                                a = GetTileAt(new Vector2(i, j));

                                if (!t.AdjacentTiles.Contains(a))
                                {
                                    t.AdjacentTiles.Add(a);
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
        }

        foreach (TileData t in tiles)
        {
            for (int i = t.X - 1; i <= t.X + 1; i++)
            {
                if (i >= 0 && i < width)
                {
                    for (int j = t.Z - 1; j <= t.Z + 1; j++)
                    {
                        if (j >= 0 && j < length)
                        {
                            a = GetTileAt(new Vector2(i, j));

                            if (!t.AllAdjacentTiles.Contains(a))
                            {
                                t.AllAdjacentTiles.Add(a);
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    //Core In-Gameplay Methods-----------------------------------------------------------------------------------------------------------------------

    private void Update()
    {
        if (!GameOver)
        {
            GameUpdate();
        }
        else
        {
            GameOverUpdate();
        }
    }

    private void GameUpdate()
    {
        if (InBuildMode)
        {
            RenderTower();
        }
        showActiveTiles();
        
        if (ObjectiveController.Instance.ShipComponent.activeSelf)
        {
            thrusterTilesOn();

        }
        
        if(GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
        {
            thrusterTilesOff();
        }
        
            if (Input.GetButtonDown("Cancel"))
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
            tm.CancelBuild();
            InBuildMode = false;
        }

        // TEMP FIX, SHOULD BE REMOVED LATER
        if (hub == null)
        {
            hub = FindObjectOfType<Hub>();
        }

        if (Input.GetButtonDown("Pause"))
        {
            pause.SetActive(!pause.activeSelf);
            SetPause(pause.activeSelf);
        }

        if (Input.GetKeyDown("c"))
        {
            ChangeCursorState();
        }

        if (resourceController.IsWin() || hubDestroyed)
        {
            Time.timeScale = 0.2f;
            MusicController.Instance.StartOutroLose();
            GameOver = true;
            InBuildMode = false;
        }
    }

    public void SetPause(bool pause)
    {
        if (pause)
        {
            Time.timeScale = 0.0f;
        }
        else
        {
            Time.timeScale = 1.0f;
        }

        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }


    private void ChangeCursorState()
    {
        switch (Cursor.lockState)
        {
            case CursorLockMode.None:
                wantedMode = CursorLockMode.Locked;
                break;
            case CursorLockMode.Locked:
                wantedMode = CursorLockMode.None;
                break;
        }
        Cursor.lockState = wantedMode;
    }

    private void GameOverUpdate()
    {
        if (GameWin)
        {
            uiController.EndGameDisplay("You win!"); //Display win UI
        }
        else
        {
            uiController.EndGameDisplay("You lose!"); //Display lose UI
        }
    }

    private void RenderTower()
    {
        TowerToSpawn = tm.GetTower("holo");

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity) && !EventSystem.current.IsPointerOverGameObject())
        {
            if (TileExistsAt(hit.point))
            {
                TileData tile = GetTileAt(hit.point);
                int x = Mathf.RoundToInt(hit.point.x);
                int y = Mathf.RoundToInt(hit.point.z);
                Vector3 spawnPos = new Vector3(x, 0, y);

                if (TowerSpawn == null)
                {
                    TowerSpawn = Instantiate(TowerToSpawn, spawnPos, Quaternion.identity);
                }
                else if (TowerSpawn != TowerToSpawn)
                {
                    Destroy(TowerSpawn);
                    TowerSpawn = Instantiate(TowerToSpawn, spawnPos, Quaternion.identity);
                }
            }
        }
        else if (TowerSpawn != null)
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
        }

        if ((Input.GetButtonDown("Cancel"))
            && (tm.GetBuildingType() != TutorialController.Instance.CurrentlyBuilding || TutorialController.Instance.TutorialStage == TutorialStage.Finished))
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
            tm.CancelBuild();
            InBuildMode = false;
        }
    }

    public bool TileExistsAt(Vector3 pos)
    {
        return TileExistsAt(new Vector2(pos.x, pos.z));
    }

    public bool TileExistsAt(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);

        return !(x >= width || x < 0 || y >= length || y < 0);

        //if (x >= width || x < 0 || y >= length || y < 0)    //with by length array, the last value will be at position (width - 1, length - 1) cause arrays love 0s.
        //{
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}
    }

    public TileData GetTileAt(Vector3 pos)
    {
        return GetTileAt(new Vector2(pos.x, pos.z));
    }

    public TileData GetTileAt(Vector2 pos)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);

        if (x >= width || x < 0 || y >= length || y < 0)    //width by length array, the last value will be at position (width - 1, length - 1) 'cause arrays love 0s.
        {
            Debug.LogError("Tile (" + x + "," + y + ") is out of range.");
            return null;
        }

        return tiles[x, y];
    }

    public void CheckTileContents(TileData tile)
    {
        Button[] buttons = uiController.buildingSelector.GetComponentsInChildren<Button>();

        if (tile.Building != null)
        {
            uiController.buildingInfo.ShowInfo(tile.Building);
            return;
        }
        else if (tile.Resource != null)
        {
            foreach (Button b in buttons)
            {
                if (b.gameObject.name != "btn_harvester")
                {
                    b.interactable = false;
                }
                else
                {
                    b.interactable = true;
                }
            }
            uiController.buildingSelector.ToggleVisibility();
        }
        else
        {
            foreach (Button b in buttons)
            {
                if (b.gameObject.name == "btn_harvester" || (b.gameObject.name == "btn_generator" && resourceController.Generators.Count >= ObjectiveController.Instance.GeneratorLimit))
                {
                    b.interactable = false;
                }
                else
                {
                    b.interactable = true;
                }
            }
            uiController.buildingSelector.ToggleVisibility();
        }
        uiController.buildingSelector.transform.position = Camera.main.WorldToScreenPoint(new Vector3(tile.X, 0, tile.Z)) - new Vector3(Screen.width / 63, -Screen.height / 25);
        UIController.instance.buildingSelector.CurrentTile = tile;
    }

    public ShipComponentState GetShipComponent(ShipComponentsEnum c)
    {
        foreach (ShipComponentState s in ShipComponents)
        {
            if (s.Component == c)
            {
                return s;
            }
        }
        return null;
    }

    public int GetRecoveredComponentCount()
    {
        int count = 0;

        foreach (ShipComponentState s in ShipComponents)
        {
            if (s.Collected)
            {
                count += 1;
            }
        }
        return count;
    }

    public void hideActiveTiles()
    {
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Tile");

        for (var i = 0; i < gameObjects.Length; i++)
            Destroy(gameObjects[i]);
    }

    public void showActiveTiles()
    {
        GameObject grids = GameObject.Find("Grids");
        
        if (index != activeTiles.Count)
        {
            hideActiveTiles();
            foreach (TileData tile in activeTiles)
            {
                Vector3 pos = Vector3.zero;
                pos.x += tile.X;
                pos.y = 0.033f;
                pos.z += tile.Z;
                tile.plane = Instantiate(WorldController.Instance.planeGridprefab, pos, Quaternion.identity);
                tile.plane.transform.SetParent(grids.transform);
            }
            index = activeTiles.Count;
        }
    }

    public void thrusterTilesOff()
    {
        if (!thrusterToggle)
        {
            List<TileData> tempDisable = new List<TileData>(DisableTiles);
            foreach (TileData tile in tempDisable)
            {
                if ((tile.X == 45) || (tile.X == 46) || (tile.X == 47))
                {
                    if ((tile.Z == 19) || (tile.Z == 20) || (tile.Z == 21))
                    {
                        activeTiles.Add(tile);
                        DisableTiles.Remove(tile);
                        tile.PowerUp(tempPower);
                    }
                }
            }
            Debug.Log("off");
            thrusterToggle = true;
        }
    }
    public void thrusterTilesOn()
    {
        if (thrusterToggle)
        {
            List<TileData> tempActive = new List<TileData>(activeTiles);
            foreach (TileData tile in tempActive)
            {
                if ((tile.X == 45) || (tile.X == 46) || (tile.X == 47))
                {
                    if ((tile.Z == 19) || (tile.Z == 20) || (tile.Z == 21))
                    {
                        DisableTiles.Add(tile);
                        activeTiles.Remove(tile);
                        if (tile.getself().Building)
                        {
                            MouseController.Instance.RemoveBulding(MouseController.Instance.ReturnCost(tile.getself()));
                        }
                        tempPower = tile.getself().PowerSource;
                        tile.PowerDown(tempPower);
                    }
                }
            }
            Debug.Log("on");
            thrusterToggle = false;
        }
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void ResetGame()
    {
        SceneManager.LoadScene("Menu");
    }
}