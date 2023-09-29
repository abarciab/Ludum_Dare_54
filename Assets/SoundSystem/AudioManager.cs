using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {
    // users assign sound clips, set the volume and pitch for each clip (optional)
    // users can assign multiple audio files for the same call, each with different volume and pitch (or syncronize them)
    // users can play sounds by calling 'play' on a PlayableSound scriptable object.
    //     settings for that sound can be found on that scriptable object

    public static AudioManager instance;

    [SerializeField] GameObject coordinatorPrefab;
    List<SoundCoordinator> soundCoordinators = new List<SoundCoordinator>();
    [SerializeField] AudioMixerGroup sfxMixerG, musicMixerG;
    [SerializeField] AudioMixer sfxMixer, musicMixer;
    [SerializeField] Vector2 volumeRange = new Vector2(0, 20);
    [SerializeField] float masterStartVol, sfxStartVol, musicStartVol;
    [Space()]
    [SerializeField] float masterVolume;
    [SerializeField] float sfxVolume, musicVolume;

    [SerializeField] bool setVolumeToStartVolume;

    private void Update()
    {
        if (setVolumeToStartVolume) {
            setVolumeToStartVolume = false;
            SetMasterVolume(masterStartVol);
            SetMusicVolume(musicStartVol);
            SetSfxVolume(sfxStartVol);
        }
    }

    private void Start()
    {
        LoadVolumeValuesFromSaveData();

        SetMasterVolume(masterStartVol);
        SetMusicVolume(musicStartVol);
        SetSfxVolume(sfxStartVol);
    }

    public void SaveVolume()
    {
        PlayerPrefs.SetFloat("masterVolume", masterVolume);
        PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
        PlayerPrefs.SetFloat("musicVolume", musicVolume);
    }

    public void ResetVolumeSaveData()
    {
    }

    void LoadVolumeValuesFromSaveData()
    {
        var master = PlayerPrefs.GetFloat("masterVolume", -100);
        if (master > 0) masterStartVol = master;
        var sfx = PlayerPrefs.GetFloat("sfxVolume", -100);
        if (sfx > 0) sfxStartVol = sfx;
        var music = PlayerPrefs.GetFloat("musicVolume", -100);
        if (music > 0) musicStartVol = music;
    }

    public AudioMixerGroup GetMixer(SoundType type)
    {
        switch (type) {
            case SoundType.sfx:
                return sfxMixerG;
            case SoundType.music:
                return musicMixerG;
        }
        return null;
    }

    public void SetMasterVolume(float vol)
    {
        masterVolume = vol;
        UpdateActualVolume();
    }

    public void SetSfxVolume(float vol)
    {
        sfxVolume = vol;
        UpdateActualVolume();
    }

    public void SetMusicVolume(float vol)
    {
        musicVolume = vol;
        UpdateActualVolume();
    }

    float MapToVolumeRange(float input)
    {
        input *= volumeRange.y - volumeRange.x;
        input += volumeRange.x;
        return Mathf.Clamp(input, volumeRange.x, volumeRange.y);
    }

    void UpdateActualVolume()
    {
        var _sfxVol = MapToVolumeRange(sfxVolume * masterVolume);
        sfxMixer.SetFloat("volume", _sfxVol);

        var _musicVol = MapToVolumeRange(musicVolume * masterVolume);
        musicMixer.SetFloat("volume", _musicVol);
    }

    

    private void Awake()
    {
        instance = this;
    }

    public void PlaySound(Sound sound, Transform caller, bool restart = true)
    {
        if (caller == null) caller = transform;
        var coordinator = GetExistingCoordinator(caller);
        coordinator.AddNewSound(sound, restart, caller != transform);
    }
    
    SoundCoordinator GetExistingCoordinator(Transform caller)
    {
        for (int i = 0; i < soundCoordinators.Count; i++) {
            var coord = soundCoordinators[i];
            if (coord == null || coord.transform.parent == null) soundCoordinators.RemoveAt(i);
        }

        foreach (var coord in soundCoordinators) {
            if (coord && coord.transform.parent == caller) return coord;
        }
        return AddNewCoord(caller);
    }

    SoundCoordinator AddNewCoord(Transform caller)
    {
        var coordObj = Instantiate(coordinatorPrefab, caller);
        var coord = coordObj.GetComponent<SoundCoordinator>();
        soundCoordinators.Add(coord);
        return coord;
    }

    private void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
