using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum SoundType { sfx, music}

[CreateAssetMenu(fileName = "New Sound", menuName = "Sound")]
public class Sound : ScriptableObject
{
    [System.Serializable]
    public class Clip {
        [HideInInspector] public string name;
        public AudioClip clip;
        public bool looping;
        public bool CustomPitchAndVolume;
        [ConditionalHide(nameof(CustomPitchAndVolume)), Range(0, 1)]
        public float volume = 1;
        [ConditionalHide(nameof(CustomPitchAndVolume)), Range(0, 2)]
        public float pitch = 1;

        public Clip()
        {
            volume = 1;
            pitch = 1;
        }

        public Clip(Clip toCopy, float volume = -1, float pitch = -1)
        {
            clip = toCopy.clip;
            this.volume = volume == -1 ? toCopy.volume : volume;
            this.pitch = pitch == -1 ? toCopy.pitch : pitch;
            looping = toCopy.looping;
        }
    }

    [SerializeField] List<Clip> clips = new List<Clip>();
    public SoundType type;
    [SerializeField] bool SynchronizePitchAndVolume = true;
    [ConditionalHide(nameof(SynchronizePitchAndVolume)), Range(0, 2)]
    [SerializeField] float pitch;
    [ConditionalHide(nameof(SynchronizePitchAndVolume)), Range(0, 2)]
    [SerializeField] float volume;
    float actualVolume;
    [SerializeField] bool voiceLines;

    [HideInInspector] public AudioSource audioSource;
    public bool instantialized;
    Vector3 sourcePos;
    bool setPos;

    public void UpdateLocalPosition(Vector3 pos)
    {
        sourcePos = pos;
        setPos = true;
    }

    public void PercentVolume(float vol, float smoothness = 1)
    {
        if (!audioSource) return;
        //Debug.Log("precent volume: " + vol);
        audioSource.volume = Mathf.Lerp(audioSource.volume, actualVolume * vol, smoothness);
    }

    public void Stop()
    {
        if (!instantialized || !audioSource) return;

        audioSource.Stop();
    }

    void Awake()
    {
        if (Application.isPlaying) instantialized = true;
    }

    public void Delete()
    {
        Destroy(audioSource);
    }

    public void PlaySilent(Transform caller = null, bool restart = true)
    {
        if (!instantialized) {
            Debug.LogError("PlaySilent() was called on an uninstatizlized Sound");
            return;
        }

        if (!audioSource) FirstTimePlay(caller, restart);
        Play(true, true);
    }

    public void SetUp(Transform caller = null, bool restart = true)
    {
        if (!instantialized) {
            Debug.LogError("SetUp() was called on an uninstatizlized Sound");
            return;
        }
        if (clips.Count == 0) return;

        if (audioSource == null) FirstTimePlay(caller, restart);
    }

    public void PlayLine(Transform speaker, int index)
    {
        if (!instantialized) {
            Debug.LogError("PlaySilent() was called on an uninstatizlized Sound");
            return;
        }

        if (!audioSource) SetUp(speaker);

        audioSource.Stop();
        if (!voiceLines) index = Random.Range(0, clips.Count);
        Play(true, index:index);
    }

    public void Play(Transform caller = null, bool restart = true)
    {
        if (!instantialized) {
            Debug.LogError("Play() was called on an uninstatizlized Sound");
            return;
        }
        if (clips.Count == 0) return;
       
        if (audioSource == null) FirstTimePlay(caller, restart);
        else Play(restart);
       
    }
    void Play(bool restart, bool silent = false, int index = 0)
    {
        var clip = GetClip();
        if (voiceLines) clip = clips[index];
        if (audioSource.isPlaying && !restart) return;

        if (setPos) audioSource.transform.localPosition = sourcePos;
        setPos = false;

        actualVolume = clip.CustomPitchAndVolume ? clip.volume : volume;
        audioSource.volume = silent ? 0 : actualVolume;
        audioSource.pitch = clip.CustomPitchAndVolume ? clip.pitch : pitch;
        audioSource.loop = clip.looping;
        audioSource.clip = clip.clip;
        audioSource.Play();
    }

    Clip GetClip()
    {
        return clips[Random.Range(0, clips.Count)];
    }

    void FirstTimePlay(Transform caller, bool restart)
    {
        var Aman = AudioManager.instance;
        if (!Aman) return;
        Aman.PlaySound(this, caller, restart);
    }

    void ConfigureSource(Clip clip, AudioSource source)
    {
        source.clip = clip.clip;
        source.volume = clip.volume;
        source.pitch = clip.pitch;
        source.loop = clip.looping;
    }
}
