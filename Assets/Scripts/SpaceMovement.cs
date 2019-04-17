using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceMovement : MonoBehaviour {
    public float movementSensitivity = 0.05f;
    //public float maxMovementDistance = 10;
    public float rotationSensitivity = 0.3f;
    public float scaleSensityvity = 0.4f;
    public float minScale = 0.5f;
    public float maxScale = 5;

    private Vector3 mousePrevPos;
    private Quaternion prevRotation;
    private Vector3 prevPos;

    private Transform content;

    private Vector3 startPos;
    private Vector3 horizontalAxis;

    private void Awake() {
        startPos = transform.position;
        var camera = Camera.main.transform;
        horizontalAxis = Vector3.Cross(camera.forward, camera.up).normalized;
        content = transform.GetChild(0);
    }

    private void Update () {
        if (!MouseController.Inst.CaptureControl(this))
            return;

        if(Input.GetMouseButtonDown(0)) {
            mousePrevPos = Input.mousePosition;
            prevRotation = transform.rotation;
        } else if(Input.GetMouseButtonDown(1)) {
            mousePrevPos = Input.mousePosition;
            prevPos = content.position;
        }
		if(Input.GetMouseButton(0)) {
            Vector3 rotByMouse = (Input.mousePosition - mousePrevPos) * rotationSensitivity;
            Quaternion vertAxisRot = Quaternion.AngleAxis(-rotByMouse.x, Vector3.up);
            Quaternion horizAxisRot = Quaternion.AngleAxis(-rotByMouse.y, horizontalAxis);
            transform.rotation = horizAxisRot * prevRotation * vertAxisRot;
        } else if (Input.GetMouseButton(1)) {
            Vector3 forwardAxis = Vector3.Cross(horizontalAxis, transform.up).normalized;
            Vector3 moveByMouse = (Input.mousePosition - mousePrevPos) * movementSensitivity;

            float scale = transform.localScale.magnitude;
            var xshift = -horizontalAxis * moveByMouse.x * movementSensitivity * scale;
            var yshift = -forwardAxis * moveByMouse.y * movementSensitivity * scale;

            var maxMovementDistance = 10;
            content.position = startPos + Vector3.ClampMagnitude(prevPos + xshift + yshift - startPos, maxMovementDistance * scale);
        }

        float curScale = transform.localScale.x * (1 + Input.GetAxis("Mouse ScrollWheel") * scaleSensityvity);
        transform.localScale = Vector3.one * Mathf.Clamp(curScale, minScale, maxScale);

        MouseController.Inst.ReleaseControl(this);
    }
}
