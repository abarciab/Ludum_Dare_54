using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControllers : MonoBehaviour
{
    [SerializeField] float moveSpeed, scrollSpeed, moveSmoothness = 0.1f, scrollSmoothness = 0.1f, lockedCameraXAngle;
    Vector3 moveDelta;
    [SerializeField] Vector2 yLimits, xRotLimits;
    [SerializeField] Vector4 posLimits;
    float scrollDelta;

    public void LockAndFrameAll(bool lockCam)
    {
        enabled = !lockCam;

        if (!lockCam) return;
        float xMid = Mathf.Lerp(posLimits.x, posLimits.y, 0.5f);
        float yMid = Mathf.Lerp(posLimits.z, posLimits.w, 0.5f);
        transform.position = new Vector3(xMid, yLimits.y, yMid);
        Camera.main.transform.localEulerAngles = new Vector3(lockedCameraXAngle, 0, 0);
    }

    private void Update()
    {
        var moveDir = GetMoveDir();

        moveDelta = Vector3.Lerp(moveDelta, moveDir, moveSmoothness);
        transform.position += moveSpeed * Time.deltaTime * moveDelta;

        float mouseDelta = Input.mouseScrollDelta.y * Time.deltaTime;
        scrollDelta = Mathf.Lerp(scrollDelta, mouseDelta, scrollSmoothness);
        if (scrollDelta < 0 && transform.localPosition.y < yLimits.y) transform.position += Vector3.up * Mathf.Abs(scrollDelta) * scrollSpeed;
        if (scrollDelta > 0 && transform.localPosition.y > yLimits.x) transform.position += Vector3.down * Mathf.Abs(scrollDelta) * scrollSpeed;
        var pos = transform.localPosition;
        pos.y = Mathf.Clamp(pos.y, yLimits.x, yLimits.y);
        transform.localPosition = pos;

        float progress = (transform.position.y - yLimits.x) / Mathf.Abs(Mathf.Abs(yLimits.x) - Mathf.Abs(yLimits.y));
        float xRot = Mathf.Lerp(xRotLimits.x, xRotLimits.y, progress);
        var current = transform.GetChild(0).localEulerAngles;
        current.x = xRot;
        transform.GetChild(0).localEulerAngles = current;
    }

    Vector3 GetMoveDir()
    {
        var dir = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) dir = transform.forward;
        if (Input.GetKey(KeyCode.S)) dir = transform.forward * -1;

        if (Input.GetKey(KeyCode.D)) dir += transform.right;
        if (Input.GetKey(KeyCode.A)) dir += transform.right * -1;
        return dir;
    }
}
