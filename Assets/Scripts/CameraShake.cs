using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake i;
    private void Awake() { i = this; } 

    Transform cam;

    float timeLeft = 0f, shakeAmount = 0.7f, startTime;

    [SerializeField] float decreaseFactor = 1.0f;

    Vector3 originalPos;

    void Start()
    {
        if (cam == null) {
            cam = Camera.main.transform;
        }
        originalPos = cam.localPosition;
    }

    public void Shake(float time = 1, float intensity = 0.5f)
    {
        shakeAmount = intensity;
        timeLeft = time;
        startTime = time;
    }

    void Update()
    {
        if (timeLeft > 0) {
            cam.localPosition = originalPos + Random.insideUnitSphere * shakeAmount * (timeLeft / startTime);

            timeLeft -= Time.deltaTime * decreaseFactor;
        }
        else {
            timeLeft = 0f;
            cam.localPosition = originalPos;
        }
    }
}
