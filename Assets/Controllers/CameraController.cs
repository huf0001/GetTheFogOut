using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Handles control of the camera

    [SerializeField]
    float moveSpeed = 4f;

    Vector3 currFramePosition;
    Vector3 lastFramePostition;

    Vector3 up;
    Vector3 right;

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
        currFramePosition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		//currFramePosition.z = 0;

        UpdateCameraMovement();

		lastFramePostition = Camera.main.ScreenToWorldPoint (Input.mousePosition);
		//lastFramePostition.z = 0;
    }

    public void UpdateCameraMovement() {
        //Update camera movement from player input.

        //Handle screen dragging
		if (Input.GetMouseButton (2) || Input.GetMouseButton (1)) {
			//Right or middle mouse
			Vector3 diff = lastFramePostition - currFramePosition;
			transform.Translate (diff);
		}

        //Camera zoom
        Camera.main.orthographicSize -= Camera.main.orthographicSize * Input.GetAxis ("Mouse ScrollWheel");
		Camera.main.orthographicSize = Mathf.Clamp (Camera.main.orthographicSize, 5f, 15f);

        //Camera movement
        Vector3 rightMovement = right * moveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
        Vector3 upMovement = up * moveSpeed * Time.deltaTime * Input.GetAxis("Vertical");

        transform.position += rightMovement;
        transform.position += upMovement;
    }
}
