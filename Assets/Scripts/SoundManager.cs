using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] playlist;
    public AudioSource audioSource;
    private int musicIndex = 0;

    public AudioMixer mainMixer;
    public float baseVolume = -30f;

    public static SoundManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            return;
        }
        instance = this;

        DontDestroyOnLoad(gameObject);

        
    }

    private void Start()
    {
        mainMixer.SetFloat("Volume", baseVolume);
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            StartCoroutine(PlayNextSong());
        }
    }

    private IEnumerator PlayNextSong()
    {

        musicIndex = (musicIndex + 1) % playlist.Length;
        audioSource.clip = playlist[musicIndex];
        audioSource.Play();

        yield return new WaitForSeconds(2f);
    }
}
