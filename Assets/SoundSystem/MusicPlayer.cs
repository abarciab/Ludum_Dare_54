using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    [SerializeField] Sound Music1, Music2, ambientWind;
    bool fadingOut;
    [SerializeField] bool muted;

    private void Start()
    {
        Music1 = Instantiate(Music1);
        Music2 = Instantiate(Music2);
        ambientWind = Instantiate(ambientWind);

        Music1.Play();
        ambientWind.Play();
        Music2.PlaySilent();
    }

    public void FadeOut()
    {
        Music2.PercentVolume(0, 0.05f);
        Music1.PercentVolume(0, 0.05f);
        ambientWind.PercentVolume(0, 0.05f);
        fadingOut = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M)) {
            muted = !muted;
        }

        if (muted) {
            Music1.PercentVolume(0);
            Music2.PercentVolume(0);
            ambientWind.PercentVolume(0);
            return;
        }

        if (fadingOut) return;

        
        Music2.PercentVolume(0, 0.05f);
        Music1.PercentVolume(1, 0.05f);
        
    }
}
