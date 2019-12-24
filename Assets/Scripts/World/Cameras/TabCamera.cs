using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabCamera : MonoBehaviour
{
    public float minFOV = 20f;
    public float maxFOV = 150f;
    public float moveSpeed = 10f;
    public float XrotateSpeed = 10f;
    public float YrotateSpeed = -10f;
    public float FOVSpeed = 10f;

    Camera attachedCamera;

    void Start()
    {
        attachedCamera = GetComponent<Camera>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        var vec = (transform.forward - Vector3.Project(transform.forward, Vector3.up)).normalized;
        if (Input.GetKey(KeyCode.W))
            transform.position += vec * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S))
            transform.position -= vec * moveSpeed * Time.deltaTime;
        vec = (transform.right - Vector3.Project(transform.right, Vector3.up)).normalized;
        if (Input.GetKey(KeyCode.D))
            transform.position += vec * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A))
            transform.position -= vec * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q))
            transform.position -= Vector3.up * moveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.E))
            transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.Z))
            attachedCamera.fieldOfView = Mathf.Clamp(attachedCamera.fieldOfView - FOVSpeed * Time.deltaTime, minFOV, maxFOV);
        if (Input.GetKey(KeyCode.X))
            attachedCamera.fieldOfView = Mathf.Clamp(attachedCamera.fieldOfView + FOVSpeed * Time.deltaTime, minFOV, maxFOV);

        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * XrotateSpeed, Space.World);
        transform.Rotate(Vector3.right * Input.GetAxisRaw("Mouse Y") * YrotateSpeed, Space.Self);
    }
}
