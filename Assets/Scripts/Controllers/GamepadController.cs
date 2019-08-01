using UnityEngine;
using UnityEngine.InputSystem;

//////////////////////////////////////////////////////////////////////////////
//
// Manages events and the input cursor (mouse or gamepad).
//
// Everything in the game should register for Start and Update events rather
// than using Unity's Magic Methods. This also allows event sinks for static
// classes and custom events.
//
// Register in Awake (and don't do anything else in Awake).
// Always un-register in OnDestroy (even if other calls trigger reg/un-reg).
//
/*
        void Awake()
        {
            EventMgr.UnityUpdateClients += onUnityUpdate;
        }
 
        void OnDestroy()
        {
            EventMgr.UnityUpdateClients -= onUnityUpdate;
        }
 
        public void onUnityUpdate()
        {
            // do something
        }
*/
//////////////////////////////////////////////////////////////////////////////

public class GamepadController : MonoBehaviour
{

    public LineRenderer CursorFLR;

    // Unity events
    public delegate void UnityStartClient();
    public delegate void UnityUpdateClient();
    public static UnityStartClient UnityStartClients;
    public static UnityUpdateClient UnityUpdateClients;

    // Input and cursor events
    public delegate void ClickButton1();
    public delegate void ClickButton2();
    public delegate void CursorMovement();
    public static ClickButton1 ClickButton1Clients;
    public static ClickButton2 ClickButton2Clients;
    public static CursorMovement CursorMovementClients;

    public static Vector3 pointerPosition;
    public static Vector3 cursorCoords;
    public static bool button1Clicked;
    public static bool button2Clicked;
    private static Vector2 previousMousePosition;

    //private FastLineRendererProperties propsFLR;
    private float cursorThickness = 0.5f;
    private float cursorSize = 1.0f;
    private float joystickTraverseScreenSecs = 2.0f;
    private float joystickPixelsPerSec;

    // Child objects
    private GameObject line1Go;
    private GameObject line2Go;
    private LineRenderer line1rend;
    private LineRenderer line2rend;

    void Start()
    {
        Cursor.visible = false;
        pointerPosition = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        previousMousePosition = Mouse.current.position.ReadValue();
        cursorCoords = Camera.main.ScreenToWorldPoint(pointerPosition);
        button1Clicked = false;
        button2Clicked = false;

        //propsFLR = new FastLineRendererProperties();
        //propsFLR.Radius = 0.25f;
        //propsFLR.LineType = FastLineRendererLineSegmentType.Full;
        //propsFLR.LineJoin = FastLineRendererLineJoin.None;

        joystickPixelsPerSec = (float)Screen.width / joystickTraverseScreenSecs;

        line1Go = new GameObject("Cursor Line 1");
        line2Go = new GameObject("Cursor Line 2");
        line1Go.transform.SetParent(this.transform);
        line2Go.transform.SetParent(this.transform);
        line1rend = line1Go.AddComponent<LineRenderer>();
        line2rend = line2Go.AddComponent<LineRenderer>();
        line1rend.positionCount = 2;
        line2rend.positionCount = 2;
        line1rend.material = new Material(Shader.Find("Sprites/Default"));
        line1rend.widthMultiplier = 0.2f;
        line1rend.useWorldSpace = true;
        line2rend.material = new Material(Shader.Find("Sprites/Default"));
        line2rend.widthMultiplier = 0.2f;
        line2rend.useWorldSpace = true;

        if (UnityStartClients != null) UnityStartClients();
    }

    void Update()
    {
        bool cursorMoved = false;

        if (Input.mousePresent)
        {
            if (Mouse.current.position.ReadValue() != previousMousePosition)
            {
                cursorMoved = true;
                pointerPosition = Mouse.current.position.ReadValue();
                previousMousePosition = Mouse.current.position.ReadValue();
            }
        }

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        if (horz != 0 || vert != 0)
        {
            cursorMoved = true;
            horz = Time.deltaTime * joystickPixelsPerSec * horz;
            vert = Time.deltaTime * joystickPixelsPerSec * vert;
            pointerPosition =
                new Vector3(
                    Mathf.Clamp(pointerPosition.x + horz, 0, Screen.width),
                    Mathf.Clamp(pointerPosition.y + vert, 0, Screen.height)
                );
        }

        if (cursorMoved)
        {
            cursorCoords = Camera.main.ScreenToWorldPoint(pointerPosition);

            Vector3[] pos = new Vector3[2];
            pos[0] = new Vector3(cursorCoords.x - cursorSize, cursorCoords.y);
            pos[1] = new Vector3(cursorCoords.x + cursorSize, cursorCoords.y);
            line1rend.SetPositions(pos);

            pos[0] = new Vector3(cursorCoords.x, cursorCoords.y - cursorSize);
            pos[1] = new Vector3(cursorCoords.x, cursorCoords.y + cursorSize);
            line2rend.SetPositions(pos);

            //Vector3 pos1 = new Vector3(cursorCoords.x - cursorSize, cursorCoords.y);
            //Vector3 pos2 = new Vector3(cursorCoords.x + cursorSize, cursorCoords.y);


            //CursorFLR.Reset();
            //propsFLR.Start = new Vector3(cursorCoords.x - cursorSize, cursorCoords.y);
            //propsFLR.End = new Vector3(cursorCoords.x + cursorSize, cursorCoords.y);
            //CursorFLR.AddLine(propsFLR);
            //propsFLR.Start = new Vector3(cursorCoords.x, cursorCoords.y - cursorSize);
            //propsFLR.End = new Vector3(cursorCoords.x, cursorCoords.y + cursorSize);
            //CursorFLR.AddLine(propsFLR);
            //CursorFLR.Apply();
        }

        button1Clicked = Input.GetMouseButtonDown(0) || Input.GetKeyDown("joystick button 0");
        button2Clicked = Input.GetMouseButtonDown(1) || Input.GetKeyDown("joystick button 1");

        if (cursorMoved && CursorMovementClients != null) CursorMovementClients();
        if (button1Clicked && ClickButton1Clients != null) ClickButton1Clients();
        if (button2Clicked && ClickButton2Clients != null) ClickButton2Clients();
        if (UnityUpdateClients != null) UnityUpdateClients();
    }
}
