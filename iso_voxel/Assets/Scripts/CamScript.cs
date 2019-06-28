using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamScript : MonoBehaviour
{
    public float speed;
    public float zoom_speed = 2;
    public float rotation_speed = 9; //must be factor of 90
    private bool is_rotating = false;
    private int rotation_step = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float unit_fwd = Input.GetAxis("MoveForward") * speed;
        Vector3 translation_fwd = Vector3.Cross(transform.right, Vector3.up);
        transform.Translate(translation_fwd * unit_fwd, Space.World);
        float unit_side = Input.GetAxis("MoveRight") * speed;
        Vector3 translation_side = Vector3.Cross(Vector3.up, transform.forward);
        transform.Translate(translation_side * unit_side, Space.World);
        Camera.main.orthographicSize += Input.GetAxis("Zoom") * zoom_speed;
        if(Camera.main.orthographicSize < 1)
        {
            Camera.main.orthographicSize = 1;
        }
        if (Input.GetButtonUp("Rotate") && !is_rotating)
        {
            is_rotating = true;
            rotation_step = 0;
        }
        if (is_rotating && rotation_step < 90.0/rotation_speed)
        {
            transform.Rotate(0, rotation_speed, 0, Space.World);
            rotation_step++;
        }
        else
        {
            is_rotating = false;
        }
        if (Input.GetButtonUp("ResetCam"))
        {
            transform.position = new Vector3(0, 10, 0);
        }
    }
}
