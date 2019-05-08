using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera camera;
    public GameObject buildingSelector;

    [SerializeField] float moveSpeed = 20f;
    [SerializeField] float rotationSpeed = 40f;
    [SerializeField] float zoomMulti = 20f;
    [SerializeField] float dragSpeed;
    [SerializeField] bool enableEdgePan = false;

    private bool isBuildingSelect;
    private Vector3 forward;
    private Vector3 right;
    private Quaternion rotation;
    private Vector3 xMove;
    private Vector3 zMove;
    private float rMove;

    // Start is called before the first frame update
    void Start()
    {

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
            float h = dragSpeed * camera.m_Lens.FieldOfView * -(Input.GetAxis("Mouse X"));
            float v = dragSpeed * camera.m_Lens.FieldOfView * -(Input.GetAxis("Mouse Y"));
            transform.Translate(h, 0, v, transform);
        }

        // Zoom
        if (Input.GetAxis("Zoom") != 0)
        {
            camera.m_Lens.FieldOfView -= Input.GetAxis("Zoom") * zoomMulti;
            camera.m_Lens.FieldOfView = Mathf.Clamp(camera.m_Lens.FieldOfView, 12f, 29f);
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
