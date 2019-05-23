using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera camera;
    [SerializeField] CinemachineVirtualCamera cameraCutscene;
    [SerializeField] bool runCameraPan;
    CinemachineFollowZoom zoom;
    public GameObject buildingSelector;

    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float rotationSpeed = 40f;
    [SerializeField] float zoomMulti = 10f;
    [SerializeField] float dragSpeed;
    [SerializeField] bool enableEdgePan = false;

    private bool isBuildingSelect;
    private Vector3 forward;
    private Vector3 right;
    private Quaternion rotation;
    private Vector3 xMove;
    private Vector3 zMove;
    private float rMove;

    private void Start()
    {
        zoom = FindObjectOfType<CinemachineFollowZoom>();

        if (runCameraPan)
        {
            cameraCutscene.gameObject.SetActive(true);
            Invoke("RunCameraPan", 7f);
        }

    }

    private void Awake()
    {
        //zoom = FindObjectOfType<CinemachineFollowZoom>();
        //cameraCutscene.gameObject.SetActive(true);
        //camera.gameObject.SetActive(false);
        //Invoke("RunCameraPan", 1f);
    }

    void RunCameraPan()
    {
        //camera.gameObject.SetActive(true);
        cameraCutscene.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBuildingSelect)
        {
            UpdateCameraMovement();
        }
    }

    void UpdateCameraMovement()
    {
        forward = transform.forward;
        right = transform.right;

        // Camera keyboard movement
        xMove = right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        zMove = forward * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");
        rMove = rotationSpeed * Time.deltaTime * Input.GetAxis("Rotation");

        transform.position += xMove;
        transform.position += zMove;
        transform.Rotate(0f, rMove, 0f);

        //Handle screen dragging
        if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
        {
            //Right or middle mouse
            float h = dragSpeed * camera.m_Lens.FieldOfView * -(Input.GetAxis("MouseX"));
            float v = dragSpeed * camera.m_Lens.FieldOfView * -(Input.GetAxis("MouseY"));
            transform.Translate(h, 0, v, transform);
        }

        // Zoom
        if (Input.GetAxis("Zoom") != 0)
        {
            //camera.m_Lens.FieldOfView -= Input.GetAxis("Zoom") * zoomMulti;
            //camera.m_Lens.FieldOfView = Mathf.Clamp(camera.m_Lens.FieldOfView, 12f, 29f);

            zoom.m_Width -= Input.GetAxis("Zoom") * zoomMulti;
            zoom.m_Width = Mathf.Clamp(zoom.m_Width, 4, 10);
        }

        Vector3 pos = transform.position;

        // Boundry checks
        pos.x = Mathf.Clamp(pos.x, 0f, WorldController.Instance.Width);
        pos.z = Mathf.Clamp(pos.z, 0f, WorldController.Instance.Length);

        transform.position = pos;
    }

    public void ToggleCameraMovement()
    {
        if (buildingSelector.activeSelf)
        {
            isBuildingSelect = false;
        }
        else
        {
            isBuildingSelect = !isBuildingSelect;
        }
    }
}
