using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using DG.Tweening;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera serialCamera;
    [SerializeField] CinemachineVirtualCamera serialCameraCutscene;
    [SerializeField] bool runCameraPan;
    private CinemachineFollowZoom zoom;

    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float zoomMulti = 10f;
    [SerializeField] float dragSpeed = 0.005f;
    //[SerializeField] bool enableEdgePan = false;

    private bool isBuildingSelect;
    private Vector3 forward;
    private Vector3 right;
    private Quaternion rotation;
    private Vector3 xMove;
    private Vector3 zMove;
    private Transform myTransform;
    private Vector3 move;
    private float zoomVal;
    private NewInputs inputs;
    private Vector3 Home;
    private bool movementEnabled = false;
    private bool finishedOpeningCameraPan = false;

    public Vector3 Move { get => move; }
    public float ZoomVal { get => zoomVal; }
    public bool MovementEnabled { get => movementEnabled; set => movementEnabled = value; }
    public bool FinishedOpeningCameraPan { get => finishedOpeningCameraPan; }

    private void Awake()
    {
        Home = new Vector3(34.77f, 0.95f, 33.4f);
    }

    private void Start()
    {
        inputs = WorldController.Instance.Inputs;

        inputs.InputMap.CameraPan.performed += ctx => move = ctx.ReadValue<Vector2>();
        inputs.InputMap.CameraPan.canceled += ctx => move = Vector2.zero;
        inputs.InputMap.Zoom.performed += ctx =>  zoomVal = ctx.ReadValue<float>();
        inputs.InputMap.Zoom.canceled += ctx => zoomVal = 0;

        myTransform = transform;

        zoom = FindObjectOfType<CinemachineFollowZoom>();

        if (runCameraPan)
        {
            serialCameraCutscene.gameObject.SetActive(true);
            Invoke(nameof(RunCameraPan), 7f);
        }

    }

    void RunCameraPan()
    {
        serialCameraCutscene.gameObject.SetActive(false);
        movementEnabled = true;
        finishedOpeningCameraPan = true;
    }

    // Update is called once per frame
    void Update()
    {
        CameraCenter();
        UpdateCameraMovement();
        zoomVal = inputs.InputMap.Zoom.ReadValue<float>();
    }

    void CameraCenter()
    {
        if (!WorldController.Instance.isGamePaused && inputs.InputMap.CameraCenter.triggered)
        {
            transform.DOMove(Home, 0.3f).Kill(true);
            //     inputs.InputMap.CameraCenter.performed += ctx => transform.DOMove(Home, 0.3f).Kill(true);
        }
    }

    float panSpeed(float zoom)
    {
        float pan = dragSpeed;
        zoom = Mathf.RoundToInt(zoom);

        if (zoom <= 17)
        {       
            if (zoom > 13 && zoom < 17)
            {
                pan = dragSpeed * 0.4f;
            }else if (zoom > 7 && zoom < 14)
            {
                pan = dragSpeed * 0.3f;
            }
            else if (zoom > 0 && zoom < 6)
            {
                pan = dragSpeed / 10;
            }
        }
        return pan; // Debug.Log("level zoom : " + zoom + "   speed: " + pan);
    }

    void UpdateCameraMovement()
    {
        if (Time.timeScale == 1 && movementEnabled)
        {
            bool hasChanged = false;
            if (move != Vector3.zero) // Only run if player is moving left/right/up/down
            {
                forward = myTransform.forward;
                right = myTransform.right;

                // Camera keyboard movement
                xMove = moveSpeed * Time.deltaTime * move.x * right;
                zMove = moveSpeed * Time.deltaTime * move.y * forward;

                var position = myTransform.localPosition;
                position += xMove;
                position += zMove;
                myTransform.localPosition = position;

                hasChanged = true;
            }

            zoomVal = ZoomVal / 3f;
            zoom.m_Width = Mathf.Lerp(zoom.m_Width, zoom.m_Width -= zoomVal * zoomMulti, 0.4f);

            if (zoom.m_Width <= 4) zoom.m_Width = 4;
            if (zoom.m_Width > 22) zoom.m_Width = 22;

            //Handle screen dragging if right click is held down
            if (Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed)
            {
              //  dragSpeed = panSpeed(zoom.m_Width);
              //  Debug.Log(zoom.m_Width + "   f  " + dragSpeed); // Right or middle mouse
                float h = panSpeed(zoom.m_Width) * serialCamera.m_Lens.FieldOfView * -inputs.InputMap.CameraDrag.ReadValue<Vector2>().x;
                float v = panSpeed(zoom.m_Width) * serialCamera.m_Lens.FieldOfView * -inputs.InputMap.CameraDrag.ReadValue<Vector2>().y;
                transform.Translate(h, 0, v, transform);
                
                hasChanged = true;
            }

            if (hasChanged)
            {
                Vector3 pos = myTransform.localPosition;
                // Boundry checks
                pos.x = Mathf.Clamp(pos.x, 0f, WorldController.Instance.Width);
                pos.z = Mathf.Clamp(pos.z, 0f, WorldController.Instance.Length);

                myTransform.localPosition = pos;
            } 
        }
    }
}
