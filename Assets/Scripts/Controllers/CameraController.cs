using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
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

    private Vector3 rightMovement;
    private Vector3 upMovement;

    // Start is called before the first frame update
    void Start()
    {
        up = transform.up;
        //up = Vector3.Normalize(up);
        //right = Quaternion.Euler(new Vector3(0, 90, 0)) * up;
        right = transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        currFramePosition = Camera.main.ScreenToViewportPoint (Input.mousePosition);
		//currFramePosition.z = 0;

        UpdateCameraMovement();

		lastFramePostition = Camera.main.ScreenToViewportPoint (Input.mousePosition);
		//lastFramePostition.z = 0;
    }

    public void UpdateCameraMovement() {
        //Update camera movement from player input.

        //Handle screen dragging
		if (Input.GetMouseButton (2) || Input.GetMouseButton (1)) {
            //Right or middle mouse
            float h = dragSpeed * Camera.main.orthographicSize * - (Input.GetAxis("Mouse X"));
            float v = dragSpeed * Camera.main.orthographicSize * - (Input.GetAxis("Mouse Y"));
            transform.Translate(h, v, 0);
        }


        //Camera zoom
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 2f, 6f);

        //Camera keyboard movement
        rightMovement = right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        upMovement = up * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");

        if (enableEdgePan)
        {
            //mouse scroll
            if (Input.mousePosition.x >= Screen.width - 20)
            {
                //scroll right
                rightMovement = right * moveSpeed * Time.deltaTime * 1;
            }
            if (Input.mousePosition.x <= 10)
            {
                //scroll left
                rightMovement = right * moveSpeed * Time.deltaTime * -1;
            }

            if (Input.mousePosition.y >= Screen.height - 20)
            {
                //scroll up
                rightMovement = up * moveSpeed * Time.deltaTime * 1;
            }
            if (Input.mousePosition.y <= 20)
            {
                rightMovement = up * moveSpeed * Time.deltaTime * -1;
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

        transform.position += rightMovement;
        transform.position += upMovement;
    }
}
