using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmbientQueue : MonoBehaviour
{

    public AudioSource source;
    [Header("Playlist")] public List<AudioClip> initiallyKnownAmbiance;

    // Start is called before the first frame update
    void Start()
    {
        PlayNext();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void PlayNext()
    {
        AudioClip clip = initiallyKnownAmbiance[UnityEngine.Random.Range(0, initiallyKnownAmbiance.Count - 1)];
        source.clip = clip;
        source.Play();
        Debug.Log("Playing ambiance sound: " + clip.name);

        float length = clip.length;
        Invoke(nameof(PlayNext), length);
    }

}
