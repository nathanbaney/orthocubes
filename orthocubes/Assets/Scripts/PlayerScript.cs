using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    bool isMoving = false;
    float speed = 3;

    public int height;

    RaycastHit hit;
    Vector3 target;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*if (isMoving)
        {
            moveToTarget();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                print(hit.distance);
                target = hit.collider.transform.parent.gameObject.transform.position;
                target = new Vector3(target.x + 2, target.y + 2, target.z + 2);
                isMoving = true;
                moveToTarget();
            }
        }
        */
    }
    void moveToTarget()
    {
        if (Vector3.Distance(transform.position, target) < 0.1)
        {
            transform.position = target;
            isMoving = false;
            height = (int)transform.position.y / 4;
            print(height);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
        }
    }
}
