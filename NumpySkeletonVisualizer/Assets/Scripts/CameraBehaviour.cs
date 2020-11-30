using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraBehaviour : MonoBehaviour
{
    [SerializeField] private float scrollSpeed = 100f;
    [SerializeField] private float moveSpeed = 100f;
    [SerializeField] private float rotateSpeed = 2f;

    public int yMinLimit = -80;
    public int yMaxLimit = 80;
    public float zoomDampening = 5.0f;

    private float xDeg = 0.0f;
    private float yDeg = 0.0f;
    private Quaternion currentRotation;
    private Quaternion desiredRotation;
    private Quaternion rotation;

    void Start()
    {
        Init();
    }

    void OnEnable()
    {
        Init();
    }

    void LateUpdate()
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
        if (Input.GetMouseButton(1))
        {
            xDeg += Input.GetAxis("Mouse X") * rotateSpeed * 0.02f;
            yDeg -= Input.GetAxis("Mouse Y") * rotateSpeed * 0.02f;

            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, 1);
            transform.rotation = rotation;
        }
    }

    private void Init()
    {
        rotation = transform.rotation;
        currentRotation = transform.rotation;
        desiredRotation = transform.rotation;

        xDeg = Vector3.Angle(Vector3.right, transform.right);
        yDeg = Vector3.Angle(Vector3.up, transform.up);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }
}
