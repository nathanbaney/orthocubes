using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public float speed;
    public float zoomSpeed = 2;
    public float rotationSpeed = 9; //must be factor of 90
    private bool isRotating = false;
    private int rotationStep = 0;
    public GameObject player;

    private int playerHeight = 0;
    // Start is called before the first frame update
    void Start()
    {
        playerHeight = player.GetComponent<PlayerScript>().height;
    }

    // Update is called once per frame
    void Update()
    {
        float unitFwd = Input.GetAxis("MoveForward") * speed;
        Vector3 translationFwd = Vector3.Cross(transform.right, Vector3.up);
        transform.Translate(translationFwd * unitFwd, Space.World);
        float unitSide = Input.GetAxis("MoveRight") * speed;
        Vector3 translationSide = Vector3.Cross(Vector3.up, transform.forward);
        transform.Translate(translationSide * unitSide, Space.World);
        Camera.main.orthographicSize += Input.GetAxis("Zoom") * zoomSpeed;
        if(Camera.main.orthographicSize < 1)
        {
            Camera.main.orthographicSize = 1;
        }
        if (Input.GetButtonUp("Rotate") && !isRotating)
        {
            isRotating = true;
            rotationStep = 0;
        }
        if (isRotating && rotationStep < 90.0/rotationSpeed)
        {
            transform.Rotate(0, rotationSpeed, 0, Space.World);
            rotationStep++;
        }
        else
        {
            isRotating = false;
        }
        if (Input.GetButtonUp("ResetCam"))
        {
            transform.position = new Vector3(0, 10, 0);
        }
        checkVisibleBlocks();

    }
    public void checkVisibleBlocks()
    {
        if(playerHeight != player.GetComponent<PlayerScript>().height)
        {   
            if(playerHeight > player.GetComponent<PlayerScript>().height)
            {
                for(int floor = playerHeight; floor > player.GetComponent<PlayerScript>().height; floor--)
                {
                    transform.parent.GetComponent<LevelScript>().setFloorVisible(floor, false);
                }
            }
            else
            {
                for (int floor = playerHeight; floor <= player.GetComponent<PlayerScript>().height; floor++)
                {
                    transform.parent.GetComponent<LevelScript>().setFloorVisible(floor, true);
                }
            }
            playerHeight = player.GetComponent<PlayerScript>().height;
        }
    }
}
