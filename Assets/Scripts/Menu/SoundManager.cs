using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public AudioClip[] playlist;
    public AudioSource audioSource;
    private int musicIndex = -1;

    public AudioMixer mainMixer;
    public float baseVolume = -30f;

    private bool isFistLoad;

    public AudioClip[] GET_PHONKY_playlist;
    bool GET_PHONKY = false;

    public static SoundManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            isFistLoad = false;
            return;
        }
        isFistLoad = true;
        instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if(isFistLoad)
            mainMixer.SetFloat("Volume", baseVolume);
    }

    private void Update()
    {
        if (!audioSource.isPlaying)
        {
            StartCoroutine(PlayNextSong());
        }

        if (Input.GetKeyUp(KeyCode.Tab))
        {
            StartCoroutine(PlayNextSong());
        }
        if (Input.GetKeyUp(KeyCode.F5))
        {
            GET_PHONKY = !GET_PHONKY;
        }
    }

    private IEnumerator PlayNextSong()
    {

        if (!GET_PHONKY)
        {
            musicIndex = (musicIndex + 1) % playlist.Length;
            audioSource.clip = playlist[musicIndex];
        }
        else
        {
            musicIndex = (musicIndex + 1) % GET_PHONKY_playlist.Length;
            audioSource.clip = GET_PHONKY_playlist[musicIndex];
        }
        audioSource.Play();

        MainMenuManager mainMenuManager = UnityEngine.Object.FindAnyObjectByType<MainMenuManager>();
        if(mainMenuManager != null)
        {
            mainMenuManager.NextBackground();
        }

        yield return new WaitForSeconds(2f);
    }
}
