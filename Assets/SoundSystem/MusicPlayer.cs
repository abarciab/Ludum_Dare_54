using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] List<Sound> tracks = new List<Sound>();
    float timeLeft, waitTime;
    [SerializeField] Vector2 silenceWaitRange = new Vector2(1, 10);
    [SerializeField] Sound ambient;
    int currentIndex;

    private void Start()
    {
        ambient = Instantiate(ambient);
        ambient.Play();

        for (int i = 0; i < tracks.Count; i++) {
            tracks[i] = Instantiate(tracks[i]);
        }
        StartNext();
    }

    public void FadeOutCurrent(float time)
    {
        timeLeft = waitTime = Mathf.Infinity;
        StartCoroutine(FadeOut(time));
    }

    IEnumerator FadeOut(float time)
    {
        float timePassed = 0;
        while (timePassed < time) {
            tracks[currentIndex].PercentVolume(Mathf.Lerp(1, 0, timePassed / time));
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
    }

    void StartNext()
    {
        currentIndex = Random.Range(0, tracks.Count);
        var selected = tracks[currentIndex];
        selected.Play();
        timeLeft = selected.GetClipLength();
        waitTime = Mathf.Infinity;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        if (timeLeft == Mathf.Infinity) waitTime -= Time.deltaTime;

        if (timeLeft < 0) {
            timeLeft = Mathf.Infinity;
            waitTime = Random.Range(silenceWaitRange.x, silenceWaitRange.y);
        }
        if (waitTime <= 0) {
            StartNext();
        }
    }

}
