using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    [SerializeField] Sound titleMusic, startSound, buttonClick;
    [SerializeField] float fadeOutTime = 4;
    [SerializeField] GameObject FadeObj;

    private void Start()
    {
        titleMusic = Instantiate(titleMusic);
        startSound = Instantiate(startSound);
        buttonClick = Instantiate(buttonClick);
        titleMusic.Play();
    }

    public void Click()
    {
        buttonClick.Play();
    }

    public void StartGame()
    {
        startSound.Play();
        StartCoroutine(FadeThenLoadGame());
        FadeObj.SetActive(true);
    }

    IEnumerator FadeThenLoadGame()
    {
        float timePassed = 0;
        while (timePassed < fadeOutTime) {
            titleMusic.PercentVolume(Mathf.Lerp(1, 0, timePassed / fadeOutTime));
            timePassed += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        
        Destroy(AudioManager.instance.gameObject);
        yield return new WaitForEndOfFrame();

        SceneManager.LoadScene(1);
    }
}
