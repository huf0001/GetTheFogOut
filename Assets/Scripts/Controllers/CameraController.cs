using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cam1;

    //Handles control of the camera

    [SerializeField]
    float moveSpeed = 20f;

    [SerializeField]
    bool enableEdgePan = false;

    float dragSpeed = 0.050f; //Don't change on a whim, specific value to making dragging work properly.

    private Vector3 currFramePosition;
    private Vector3 lastFramePostition;
    private Vector3 dragOrigin;

    private Vector3 up;
    private Vector3 right;

    private Vector3 xMove;
    private Vector3 yMove;

    private Vector3 centerPoint;
    private Vector3 defaultRotation;
    private Vector3 defaultPosition;
    private Vector3 rotateCam;
    private int counter;

    private bool isBuildingSelect;

    // Start is called before the first frame update
    void Start()
    {
        up = transform.up;
        //up = Vector3.Normalize(up);
        //right = Quaternion.Euler(new Vector3(0, 90, 0)) * up;
        right = transform.right;

        defaultRotation = cam1.transform.eulerAngles;
        defaultPosition = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z);
        counter = 1;
        isBuildingSelect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isBuildingSelect)
        {
            currFramePosition = Camera.main.ScreenToViewportPoint (Input.mousePosition);
		    //currFramePosition.z = 0;

            UpdateCameraMovement();

            lastFramePostition = Camera.main.ScreenToViewportPoint (Input.mousePosition);
            //lastFramePostition.z = 0;
            RotateCamera();
        }
    }

    public void ToggleCameraMovement()
    {
        isBuildingSelect = !isBuildingSelect;
    }

    public void RotateCamera()
    {
        Vector3 newPos = defaultPosition;
        if (Input.GetKeyDown(KeyCode.E))
        {
            switch (counter)
            {
                case 0:
                    break;
                case 1:
                    newPos.z += 53.2f;
                    break;
                case 2:
                    newPos.z += 49.5f;
                    newPos.x += 52.4f;
                    break;
                case 3:
                    newPos.x += 48.7f;
                    newPos.z -= 0.8f;
                    break;
            }
            counter++;
            rotateCam.y = 90f;

            if (counter == 5)
            {
                counter = 1;
                rotateCam = Vector3.zero;
                cam1.transform.eulerAngles = defaultRotation;
            }
            cam1.transform.Rotate(rotateCam, 20.0f * Time.deltaTime);
            cam1.transform.eulerAngles += rotateCam;
            transform.position = newPos;
            up = cam1.transform.up;
            right = cam1.transform.right;
        }
    }


    public void UpdateCameraMovement() {
        //Update camera movement from player input.

        //Handle screen dragging
		if (Input.GetMouseButton (2) || Input.GetMouseButton (1)) {
            //Right or middle mouse
            float h = dragSpeed * Camera.main.orthographicSize * - (Input.GetAxis("Mouse X"));
            float v = dragSpeed * Camera.main.orthographicSize * - (Input.GetAxis("Mouse Y"));
            transform.Translate(h, v, 0,Space.World);
        }


        //Camera zoom
        cam1.m_Lens.OrthographicSize -= cam1.m_Lens.OrthographicSize * Input.GetAxis("Mouse ScrollWheel");
        cam1.m_Lens.OrthographicSize = Mathf.Clamp(cam1.m_Lens.OrthographicSize, 2f, 6f);

        //Camera keyboard movement
        xMove = right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        yMove = up * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");

        if (enableEdgePan)
        {
            //mouse scroll
            if (Input.mousePosition.x >= Screen.width - 20)
            {
                //scroll right
                xMove = right * moveSpeed * Time.deltaTime * 1;
            }
            if (Input.mousePosition.x <= 10)
            {
                //scroll left
                xMove = right * moveSpeed * Time.deltaTime * -1;
            }

            if (Input.mousePosition.y >= Screen.height - 20)
            {
                //scroll up
                xMove = up * moveSpeed * Time.deltaTime * 1;
            }
            if (Input.mousePosition.y <= 20)
            {
                xMove = up * moveSpeed * Time.deltaTime * -1;
            }

            if (Input.GetKey("q"))
            {
                //scroll down
                transform.Rotate(Vector3.forward * moveSpeed * Time.deltaTime);
            }

            if (Input.GetKey("e"))
            {
                transform.Rotate(Vector3.forward * -moveSpeed * Time.deltaTime);
            }
        }

        transform.position += xMove;
        transform.position += yMove;
    }
    /* //no use for now?
    public void getCenter()
    {
        var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.height / 2, Screen.width / 2));
        RaycastHit hitPoint;

        if (Physics.Raycast(ray, out hitPoint, 100.0f))
        {
            transform.LookAt(hitPoint.point);

            if (centerPoint == Vector3.zero)
            {
                centerPoint = hitPoint.point;
            }
        }
    }
    */
}
