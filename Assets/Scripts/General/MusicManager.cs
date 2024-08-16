using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    public GameObject temporalAudioPlayerPrefab;
    private AudioSource _referenceAudioSource;

    [Header("Nutzereinstellungen (nur lesen)")] [Range(0, 1)]
    public static float userDesiredMusicVolume = 0.5f;

    [Range(0, 1)] public static float userDesiredSoundVolume = 0.5f;
    [Range(0, 1)] public static float userDesiredMasterVolume = 0.5f;

    [Header("Basis Einstellungen")] [Range(0, 1)]
    public float baselineMusicVolume = 1.0f;

    [Range(0, 1)] public float baselineSoundVolume = 1.0f;
    [Range(0, 1)] public float baselineMasterVolume = 1.0f;

    public float userDesiredMusicVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredMusicVolume * baselineMusicVolume, 0.0001f, 1.0f)) * 20;

    public float userDesiredSoundVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredSoundVolume * baselineSoundVolume, 0.0001f, 1.0f)) * 20;

    public float userDesiredMasterVolumeDB =>
        Mathf.Log10(Mathf.Clamp(userDesiredMasterVolume * baselineMasterVolume, 0.0001f, 1.0f)) * 20;

    [Header("Mixer Namen")] public AudioMixer audioMixer;
    public string masterTrackName = "master";
    public string musicTrackName = "music";
    public string soundEffectsTrackName = "sfx";

    [Header("Einstellungen")] public float binningVolumeMult = 0.15f;
    public float musicFadeSpeed = 1;

    [Header("Playlist")] public List<AudioClip> initiallyKnownSongs;
    private AudioListener _listener;

    // Private fields
    private List<AudioSource> _playList;
    private List<int> _desiredMixingVolumes;
    private Dictionary<string, float> _audioJail;

    private void Awake()
    {
        _playList = new List<AudioSource>();
        _desiredMixingVolumes = new List<int>();
        _referenceAudioSource = GetComponent<AudioSource>();

        foreach (AudioClip songClip in initiallyKnownSongs)
        {
            AudioSource songSource = CreatePlaylistInstance(songClip);

            _playList.Add(songSource);
            songSource.Play();
            songSource.volume = 0;
            _desiredMixingVolumes.Add(0);
        }

        _referenceAudioSource.enabled = false;
        SkipFade();

        _audioJail = new Dictionary<string, float>();
    }

    // Start is called before the first frame update
    void Start()
    {
        FindAudioListener();
    }

    private AudioSource CreatePlaylistInstance(AudioClip songClip)
    {
        // Creating empty gameobject
        GameObject songInstance = new GameObject(songClip.name);
        songInstance.transform.parent = transform;
        songInstance.transform.localPosition = Vector3.zero;

        // adding clip component
        AudioSource newSource = songInstance.AddComponent<AudioSource>();
        CopyAudioSourceValues(_referenceAudioSource, newSource);

        // Using c# reflections to copy values and flags from reference
        // BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        // FieldInfo[] fields = typeof(AudioSource).GetFields(flags);
        // foreach (FieldInfo field in fields)
        // {
        //     // Check if the field is serializable by Unity
        //     bool isPublic = field.IsPublic;
        //     bool hasSerializeField = field.GetCustomAttribute<SerializeField>() != null;
        //     if (isPublic || hasSerializeField)
        //     {
        //         field.SetValue(newSource, field.GetValue(_referenceAudioSource));
        //     }
        // }

        newSource.clip = songClip;
        return newSource;
    }

    public void Play(int index, bool fromBeginning = false)
    {
        for (var i = 0; i < _playList.Count; i++)
        {
            _desiredMixingVolumes[i] = 0;
        }

        if (fromBeginning)
        {
            _playList[index].time = 0;
        }

        _desiredMixingVolumes[index] = 1;

        if (!_playList[index].isPlaying)
        {
            _playList[index].Play();
        }

        if (fromBeginning || index != CurrentlyPlayingMusicIndex())
        {
            Debug.Log("Playing Song: " + _playList[index].gameObject.name);
        }
    }

    public int CurrentlyPlayingMusicIndex()
    {
        for (var i = 0; i < _desiredMixingVolumes.Count; i++)
        {
            int desiredMixingVolume = _desiredMixingVolumes[i];
            if (desiredMixingVolume == 1)
            {
                return i;
            }
        }

        return -1;
    }

    public void SkipFade()
    {
        for (var i = 0; i < _playList.Count; i++)
        {
            _playList[i].volume = _desiredMixingVolumes[i];
        }
    }

    // Update is called once per frame
    void Update()
    {
        audioMixer.SetFloat(musicTrackName, userDesiredMusicVolumeDB);
        audioMixer.SetFloat(soundEffectsTrackName, userDesiredSoundVolumeDB);
        audioMixer.SetFloat(masterTrackName, userDesiredMasterVolumeDB);
        _referenceAudioSource.enabled = false;

        if (_audioJail == null) return;

        if (_listener.IsDestroyed())
        {
            FindAudioListener();
        }

        transform.position = _listener.transform.position;
        userDesiredSoundVolume = MathF.Min(userDesiredMusicVolume * 1.0f, 1.0f);

        for (var i = 0; i < _playList.Count; i++)
        {
            var audioSource = _playList[i];
            var volumeMixing = _desiredMixingVolumes[i];

            var trueVolume = Mathf.MoveTowards(audioSource.volume,
                volumeMixing,
                Time.deltaTime * musicFadeSpeed);

            if (trueVolume - Time.deltaTime * musicFadeSpeed <= 0 && volumeMixing == 0)
            {
                trueVolume = 0;
            }

            audioSource.volume = trueVolume;
        }

        var keys = _audioJail.Keys.ToArrayPooled().ToList();
        List<string> releaseKeys = new List<string>();
        if (keys.Count > 0)
        {
            for (var i = 0; i < keys.Count; i++)
            {
                string key = keys[i];
                float timeout = _audioJail[key];
                timeout -= Time.deltaTime;
                _audioJail[key] = timeout;

                if (timeout < 0)
                {
                    releaseKeys.Add(key);
                }
            }
        }

        foreach (var releaseKey in releaseKeys)
        {
            _audioJail.Remove(releaseKey);
        }

        string pg = "";
        foreach (var audioSource in _playList)
        {
            pg += " - " + audioSource.time;
        }
    }

    private void LateUpdate()
    {
        userDesiredMusicVolume = Mathf.Clamp(userDesiredMusicVolume, 0f, 1f);
        userDesiredSoundVolume = Mathf.Clamp(userDesiredSoundVolume, 0f, 1f);
        userDesiredMasterVolume = Mathf.Clamp(userDesiredMasterVolume, 0f, 1f);
        baselineMusicVolume = Mathf.Clamp(baselineMusicVolume, 0f, 1f);
        baselineSoundVolume = Mathf.Clamp(baselineSoundVolume, 0f, 1f);
        baselineMasterVolume = Mathf.Clamp(baselineMasterVolume, 0f, 1f);
    }

    public float GetVolumeMusic()
    {
        audioMixer.GetFloat(musicTrackName, out float volume);
        return volume;
    }

    public float GetVolumeSound()
    {
        audioMixer.GetFloat(soundEffectsTrackName, out float volume);
        return volume;
    }

    public float GetMasterSound()
    {
        audioMixer.GetFloat(masterTrackName, out float volume);
        return volume;
    }

    public GameObject CreateAudioClip(AudioClip audioClip,
        Vector3 position,
        float pitchRange = 0.0f,
        float soundVolume = 1.0f,
        bool threeDimensional = false,
        bool respectBinning = false)
    {
        // Registering in the jail
        string clipName = audioClip.name;
        float jailTime = audioClip.length * 0.42f;
        float binningMult = 1.0f;

        if (_audioJail.ContainsKey(clipName))
        {
            _audioJail[clipName] = jailTime;
            if (respectBinning)
            {
                binningMult = binningVolumeMult;
                // return;
            }
        }
        else
        {
            _audioJail.Add(clipName, jailTime);
        }

        // Instancing the sound
        GameObject soundInstance = Instantiate(temporalAudioPlayerPrefab);
        soundInstance.transform.position = position;
        AudioSource source = soundInstance.GetComponent<AudioSource>();
        TimedLife life = soundInstance.GetComponent<TimedLife>();
        life.aliveTime = audioClip.length * 2;

        if (threeDimensional)
        {
            source.spatialBlend = 1;
        }
        else
        {
            source.spatialBlend = 0;
        }

        source.clip = audioClip;
        source.pitch = 1.0f + Random.Range(-pitchRange, pitchRange);
        source.volume = MathF.Min(soundVolume * binningMult, 1.0f);
        source.Play();

        return soundInstance;
    }

    public float AudioBinExternalSoundMult(AudioClip audioClip)
    {
        string clipName = audioClip.name;
        float jailTime = audioClip.length * 0.42f;
        if (_audioJail.ContainsKey(clipName))
        {
            return binningVolumeMult;
        }
        else
        {
            _audioJail.Add(clipName, jailTime);
            return 1.0f;
        }
    }

    private void CopyAudioSourceValues(AudioSource source, AudioSource target)
    {
        target.clip = source.clip;
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.mute = source.mute;
        target.bypassEffects = source.bypassEffects;
        target.bypassListenerEffects = source.bypassListenerEffects;
        target.bypassReverbZones = source.bypassReverbZones;
        target.playOnAwake = source.playOnAwake;
        target.loop = source.loop;
        target.priority = source.priority;
        target.volume = source.volume;
        target.pitch = source.pitch;
        target.panStereo = source.panStereo;
        target.spatialBlend = source.spatialBlend;
        target.reverbZoneMix = source.reverbZoneMix;
        target.dopplerLevel = source.dopplerLevel;
        target.spread = source.spread;
        target.rolloffMode = source.rolloffMode;
        target.minDistance = source.minDistance;
        target.maxDistance = source.maxDistance;

        // Apply 3D sound settings
        target.spatialize = source.spatialize;
        target.spatializePostEffects = source.spatializePostEffects;
        target.SetCustomCurve(AudioSourceCurveType.CustomRolloff,
            source.GetCustomCurve(AudioSourceCurveType.CustomRolloff));
        target.SetCustomCurve(AudioSourceCurveType.SpatialBlend,
            source.GetCustomCurve(AudioSourceCurveType.SpatialBlend));
        target.SetCustomCurve(AudioSourceCurveType.ReverbZoneMix,
            source.GetCustomCurve(AudioSourceCurveType.ReverbZoneMix));
        target.SetCustomCurve(AudioSourceCurveType.Spread, source.GetCustomCurve(AudioSourceCurveType.Spread));

        // Copy 3D sound settings
        target.spatialize = source.spatialize;
        target.spatialBlend = source.spatialBlend;
        target.dopplerLevel = source.dopplerLevel;
        target.spread = source.spread;
        target.rolloffMode = source.rolloffMode;
        target.minDistance = source.minDistance;
        target.maxDistance = source.maxDistance;

        Debug.Log("AudioSource values copied from source to target.");
    }

    private void FindAudioListener()
    {
        _listener = FindObjectOfType<AudioListener>();
    }
}