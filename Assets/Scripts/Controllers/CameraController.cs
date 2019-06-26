using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera serialCamera;
    [SerializeField] CinemachineVirtualCamera serialCameraCutscene;
    [SerializeField] bool runCameraPan;
    private CinemachineFollowZoom zoom;
    public GameObject buildingSelector;

    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float rotationSpeed = 40f;
    [SerializeField] float zoomMulti = 10f;
    [SerializeField] float dragSpeed;
    //[SerializeField] bool enableEdgePan = false;

    private bool isBuildingSelect;
    private Vector3 forward;
    private Vector3 right;
    private Quaternion rotation;
    private Vector3 xMove;
    private Vector3 zMove;
    private float rMove;
    private Transform myTransform;

    private void Start()
    {
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
        //camera.gameObject.SetActive(true);
        serialCameraCutscene.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCameraMovement();
    }

    void UpdateCameraMovement()
    {
        bool hasChanged = false;
        //Only run if player is moving left/right/up/down
        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            forward = myTransform.forward;
            right = myTransform.right;

            // Camera keyboard movement
            xMove = moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal") * right;
            zMove = moveSpeed * Time.deltaTime * Input.GetAxis("Vertical") * forward;
            
            var position = myTransform.localPosition;
            position += xMove;
            position += zMove;
            myTransform.localPosition = position;

            hasChanged = true;
        }
        
        //Only run if player is moving left/right/up/down
        if (Input.GetAxisRaw("Rotation") != 0)
        {
            rMove = rotationSpeed * Time.deltaTime * Input.GetAxis("Rotation");
            myTransform.Rotate(0f, rMove, 0f);
        }

        //Handle screen dragging if right click is held down
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            //Right or middle mouse
            float h = dragSpeed * serialCamera.m_Lens.FieldOfView * -(Input.GetAxis("MouseX"));
            float v = dragSpeed * serialCamera.m_Lens.FieldOfView * -(Input.GetAxis("MouseY"));
            transform.Translate(h, 0, v, transform);

            hasChanged = true;
            
        }

        // Zoom
        if (Input.GetAxis("Zoom") != 0)
        {
            //camera.m_Lens.FieldOfView -= Input.GetAxis("Zoom") * zoomMulti;
            //camera.m_Lens.FieldOfView = Mathf.Clamp(camera.m_Lens.FieldOfView, 12f, 29f);

            zoom.m_Width -= Input.GetAxis("Zoom") * zoomMulti;
            zoom.m_Width = Mathf.Clamp(zoom.m_Width, 4, 10);
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
