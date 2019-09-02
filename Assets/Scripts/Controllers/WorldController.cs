using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
    [SerializeField] private bool gameWin = false, gameOver = false;

    [Header("Prefab/Gameobject assignment")]
    [SerializeField] private GameObject ground;

    [SerializeField] public GameObject planeGridprefab, minimapPlanePrefab/*, hubPrefab, *//*mineralPrefab*/;
    [SerializeField] public Material normalTile, hoverTile, collectibleTile;

    [SerializeField] private Hub hub = null;
    [SerializeField] private TileData[,] tiles;
    [SerializeField] private List<ShipComponentState> shipComponents = new List<ShipComponentState>();
    [SerializeField] protected GameObject serialCamera;
    [SerializeField] private AbilityMenu abilityMenu;
    [SerializeField] private GameObject mainCamera;

    [Header("Public variable?")]
    public bool InBuildMode;
    public GameObject pauseMenu;

    private MusicFMOD musicFMOD;
    public MusicFMOD musicfmod;

    private FMOD.Studio.Bus musicBus;
    private float musicVolume = 1f;

    public Upgrade hvstUpgradeLevel;
    public Upgrade mortarUpgradeLevel;
    public Upgrade pulseDefUpgradeLevel;

    //Non-Serialized Fields
    private GameObject temp, PlaneSpawn, TowerSpawn, TowerToSpawn, tiletest, tmp;
    private GameObject[] objs;
    private TowerManager tm;
    private Vector3 pos;
    private Camera cam;

    //Other Controllers
    private ResourceController resourceController;
    private UIController uiController;

    //private List<TileData> ThrusterList = new List<TileData>();

    private List<TileData> activeTiles = new List<TileData>();
    public List<TileData> ActiveTiles { get => activeTiles; }

    private List<TileData> disableTiles = new List<TileData>();
    public List<TileData> DisableTiles { get => disableTiles; }

    //Cursor Locking to centre
    private CursorLockMode wantedMode;

    //Flags
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
    public NewInputs Inputs { get; set; }

    //Start-Up Methods-------------------------------------------------------------------------------------------------------------------------------
    private void Start()
    {
        index = 0;
        InstantiateTileArray();
        ConnectAdjacentTiles();
        SetResourcesToTiles();
        SetBuildingsToTiles();
        SetLandmarksToTiles();
        SetCollectablesToTiles();
        SetRocksToTiles();
        CreateMinimapTiles();
        TutorialController.Instance.StartTutorial();

        if (GameObject.Find("MusicFMOD") != null)
        {
            musicFMOD = GameObject.Find("MusicFMOD").GetComponent<MusicFMOD>();
        }
        else
        {
            Instantiate(musicfmod);
            musicFMOD = musicfmod;
        }
        musicFMOD.StartMusic();
        musicFMOD.StageOneMusic();
        musicBus = FMODUnity.RuntimeManager.GetBus("bus:/MASTER/MUSIC");
    }

    private void Awake()
    {
        Inputs = new NewInputs();
        Inputs.Enable();
        Inputs.InputMap.Pause.performed += ctx => SetPause(!pauseMenu.activeSelf);

        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more world managers.");
        }

        InBuildMode = false;
        Instance = this;
        cam = Camera.main;

        tm = FindObjectOfType<TowerManager>();

        Cursor.lockState = wantedMode;
        Cursor.visible = (CursorLockMode.Locked != wantedMode);

        serialCamera = GameObject.Find("CameraTarget");
        uiController = GetComponent<UIController>();
        resourceController = ResourceController.Instance;
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

    void SetCollectablesToTiles()
    {
        Collectable[] collectables = FindObjectsOfType<Collectable>();
        foreach (Collectable c in collectables)
        {
            var position = c.transform.position;
            TileData tile = GetTileAt(position);
            tile.Collectible = c;
            tile.buildingChecks.collectable = true;
            c.Location = tile;

            // Centre on tile
            Vector3 pos = position;
            pos.x = Mathf.Round(pos.x);
            pos.z = Mathf.Round(pos.z);
            position = pos;
            c.transform.position = position;
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

    void SetRocksToTiles()
    {
        TileBlock[] tileBlocks = FindObjectsOfType<TileBlock>();

        foreach (TileBlock tileBlock in tileBlocks)
        {
            TileData tileData = GetTileAt(tileBlock.transform.position);
            tileData.buildingChecks.obstacle = true;
        }
    }

    private void InstantiateTileArray()
    {
        tiles = new TileData[width, length];
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < length; z++)
            {
                TileData tile = new TileData(x, z);
                tiles[x, z] = tile;
                pos = new Vector3(tile.X, 0, tile.Z);
            }
        }
    }

    private void CreateMinimapTiles()
    {
        GameObject grids = GameObject.Find("MinimapPlanes");
        foreach (TileData tile in tiles)
        {
            Vector3 pos = Vector3.zero;
            pos.x += tile.X;
            pos.y = 0.033f;
            pos.z += tile.Z;
            GameObject minimapTile = Instantiate(minimapPlanePrefab, pos, minimapPlanePrefab.transform.localRotation);
            minimapTile.transform.SetParent(grids.transform);
            minimapTile.GetComponent<MinimapTile>().Tile = tile;
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
            if (GameWin)
            {
                StartCoroutine("PlayWinAnimator");
            }
            else
            {
                StartCoroutine("PlayDeadAnimator");
            }
            //   GameOverUpdate();
        }
    }

    private void GameUpdate()
    {
        if (Gamepad.all.Count > 0 && Gamepad.current.wasUpdatedThisFrame)
        {
            //Cursor.lockState = CursorLockMode.Locked;
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2, Screen.height / 2));
        }
        else
        {
            //Cursor.lockState = CursorLockMode.None;
        }

        if (InBuildMode)
        {
            RenderTower();
        }
        showActiveTiles();

        if (ObjectiveController.Instance.thruster.activeSelf)
        {
            thrusterTilesOn();

        }

        if (GetShipComponent(ShipComponentsEnum.Thrusters).Collected)
        {
            thrusterTilesOff();
        }

        if (Inputs.InputMap.Pause.triggered)
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

        //if (Input.GetButtonDown("Pause"))
        //{
        //    SetPause(!pauseMenu.activeSelf);
        //}

        if (Input.GetKeyDown("c"))
        {
            ChangeCursorState();
        }

        if (hubDestroyed)
        {
            Time.timeScale = 0.2f;
            GameOver = true;
            InBuildMode = false;
        }
    }

    /// <summary>
    /// To replace EventSystem.current.IsPointerOverGameObject() since it is not compatible with the new Input System
    /// </summary>
    /// <returns>Returns true if pointer over UI object</returns>
    public bool IsPointerOverGameObject()
    {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, results);

        return results.Count > 0 ? true : false;
    }

    public void SetPause(bool pause)
    {
        if (!uiController.buildingSelector.Visible && !uiController.buildingInfo.Visible && !abilityMenu.Visible)
        {
            pauseMenu.SetActive(!pauseMenu.activeSelf);

            if (pause)
            {
                Time.timeScale = 0.0f;
                musicVolume = 0.3f;
            }
            else
            {
                Time.timeScale = 1.0f;
                musicVolume = 1f;
            }

            musicBus.setVolume(musicVolume);
        }
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

    IEnumerator PlayDeadAnimator()
    {
        Animator dead = mainCamera.GetComponent<Animator>();
        if (dead)
        {
            dead.SetBool("IsDead", true);
        }
        yield return new WaitForSeconds(1f);
        GameOverUpdate();
    }

    IEnumerator PlayWinAnimator()
    {
        Animator win = Hub.Instance.Animator;
        if (win)
        {
            win.SetBool("Win", true);
        }
        yield return new WaitForSeconds(8.0f);
        GameWinUpdate();
    }

    private void GameWinUpdate()
    {
        musicFMOD.GameWinMusic();
        uiController.EndGameDisplay("You win!"); //Display win UI
    }

    private void GameOverUpdate()
    {
        musicFMOD.GameLoseMusic();
        uiController.EndGameDisplay("You lose!"); //Display lose UI     
    }

    private void RenderTower()
    {
        TowerToSpawn = tm.GetTower("holo");

        RaycastHit hit;
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Instance.Ground.GetComponent<Collider>().Raycast(ray, out hit, Mathf.Infinity) && !IsPointerOverGameObject())
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

        if (Inputs.InputMap.Pause.triggered && (tm.GetBuildingType() != TutorialController.Instance.CurrentlyBuilding || TutorialController.Instance.Stage == TutorialStage.Finished))
        {
            Destroy(PlaneSpawn);
            Destroy(TowerSpawn);
            tm.CancelBuild();
            InBuildMode = false;
        }
    }

    public bool TileExistsAt(Vector2 position)
    {
        return TileExistsAt(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public bool TileExistsAt(Vector3 position)
    {
        return TileExistsAt(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
    }

    public bool TileExistsAt(int x, int y)
    {
        return x < width && x >= 0 && y < length && y >= 0;
    }

    public TileData GetTileAt(Vector2 position)
    {
        return GetTileAt(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));
    }

    public TileData GetTileAt(Vector3 position)
    {
        return GetTileAt(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.z));
    }

    public TileData GetTileAt(int x, int y)
    {
        if (x >= width || x < 0 || y >= length || y < 0)
        {
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
        uiController.buildingSelector.transform.position = cam.WorldToScreenPoint(new Vector3(tile.X, 0, tile.Z)) + new Vector3(12, 5); // + new Vector3(-Screen.width / 100, Screen.height / 25);
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
        if (index != activeTiles.Count)
        {
            GameObject grids = GameObject.Find("Grids");

            hideActiveTiles();

            foreach (TileData tile in activeTiles)
            {
                if (!tile.buildingChecks.obstacle)
                {
                    Vector3 pos = Vector3.zero;
                    pos.x += tile.X;
                    pos.y = 0.033f;
                    pos.z += tile.Z;
                    tile.plane = Instantiate(planeGridprefab, pos, planeGridprefab.transform.localRotation);

                    tile.plane.transform.SetParent(grids.transform);
                    if (tile.buildingChecks.collectable)
                    {
                        MeshRenderer mesh = tile.plane.GetComponent<MeshRenderer>();
                        mesh.material = collectibleTile;
                    }
                }
            }

            index = activeTiles.Count;
        }
    }

    public void abilityTilestoggle(TileData Tile, bool toggle)
    {
        if (toggle)
        {
            List<TileData> tempActive = new List<TileData>(activeTiles);
            foreach (TileData tile in tempActive)
            {
                if (Tile == tile)
                {
                    DisableTiles.Add(tile);
                    activeTiles.Remove(tile);
                    break;
                }
            }
        }
        else
        {
            List<TileData> tempDisable = new List<TileData>(DisableTiles);
            foreach (TileData tile in tempDisable)
            {
                if (Tile == tile)
                {
                    if (!activeTiles.Contains(tile))
                    {
                        activeTiles.Add(tile);
                    }
                    DisableTiles.Remove(tile);
                    break;
                }
            }
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
                        if (!activeTiles.Contains(tile))
                        {
                            activeTiles.Add(tile);
                        }
                        DisableTiles.Remove(tile);
                        tile.PowerUp(tempPower);
                        //             Debug.Log(tile.Name);
                    }
                }
            }
            //    Debug.Log("off");
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
                        //     Debug.Log(tile.Name);
                    }
                }
            }
            //    Debug.Log("on");
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
        musicBus.setVolume(1f);
        musicFMOD.OutroMusic();
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
        SceneManager.LoadScene("Menu");
    }

    private void OnDisable()
    {
        Inputs.Disable();
        Inputs = null;
    }
}