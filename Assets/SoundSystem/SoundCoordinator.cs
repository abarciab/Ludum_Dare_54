using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundCoordinator : MonoBehaviour
{
    public void AddNewSound(Sound sound, bool restart, bool _3D = true)
    {
        var source = gameObject.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = AudioManager.instance.GetMixer(sound.type);
        source.playOnAwake = false;
        if (_3D) source.spatialBlend = 1;
        sound.audioSource = source;
        sound.Play(transform.parent, restart);
    }

}
