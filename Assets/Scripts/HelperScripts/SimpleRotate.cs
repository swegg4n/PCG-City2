using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleRotate : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 10;

    void FixedUpdate()
    {
        gameObject.transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }
}
