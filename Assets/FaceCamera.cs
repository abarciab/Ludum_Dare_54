using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    [SerializeField] bool yOnly = true;

    private void OnEnable()
    {
        Face();
    }

    void Update()
    {
        Face();
    }

    void Face()
    {
        var rot = transform.localEulerAngles;
        transform.LookAt(Camera.main.transform);
        if (!yOnly) return;

        rot.y = transform.localEulerAngles.y;
        transform.localEulerAngles = rot;
    }
}
