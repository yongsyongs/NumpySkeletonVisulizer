using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float rotateSpeed = 2f;

    void Update()
    {
        Vector3 pos = transform.position;

        // moving by mouse scroll
        pos += transform.InverseTransformDirection(transform.forward) * Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

        // moving by axis key input
        float F = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0); 
        float R = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0); 
        float U = (Input.GetKey(KeyCode.Space) ? 1 : 0) - (Input.GetKey(KeyCode.LeftShift) ? 1 : 0);

        pos += (transform.up * U + transform.right * R + transform.forward * F) * moveSpeed * Time.deltaTime;

        transform.position = pos;

        // rotating by mouse drag
        if (Input.GetMouseButton(2))
        {
            transform.Rotate(transform.up, Input.GetAxis("Mouse X") * rotateSpeed);
            transform.Rotate(transform.right, Input.GetAxis("Mouse Y") * rotateSpeed);
        }
    }
}
