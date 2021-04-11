using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //public bool canMove = false;

    float speed;

    float walkSpeed = 15.0f;
    float defaultSpeed = 50.0f;
    float sprintSpeed = 150.0f;
    float camSens = 0.7f;


    float mouseX = 0;
    float mouseY = 0;


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //canMove = !canMove;
            //Cursor.lockState = canMove ? CursorLockMode.Locked : CursorLockMode.None;

            //mouseX = transform.eulerAngles.y;
        }

       // if (!canMove)
       //     return;

        mouseX += Input.GetAxis("Mouse X") * camSens;
        mouseY += -Input.GetAxis("Mouse Y") * camSens;

        transform.eulerAngles = new Vector3(mouseY, mouseX, 0) * camSens;


        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            p *= sprintSpeed;
        }
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            p *= walkSpeed;
        }
        else
        {
            p *= defaultSpeed;
        }


        p *= Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }
    }

    private Vector3 GetBaseInput()
    {
        Vector3 p_Velocity = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}
