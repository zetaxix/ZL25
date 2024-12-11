using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuMusicManager : MonoBehaviour
{
    // Singleton instance
    public static MenuMusicManager Instance;

    // AudioSource for playing music
    private AudioSource audioSource;

    // List of AudioClips to play
    public List<AudioClip> musicClips;

    // Slider to control music volume
    private Slider settingsMusicSlider;

    // Double-tap detection variables
    private float lastTapTime;
    private const float doubleTapThreshold = 0.3f; // Maximum time (in seconds) between two taps

    private void Awake()
    {
        // Implement Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Prevent destruction on scene load
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
            return;
        }

        // Initialize AudioSource component
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        // Start playing music if the list is not empty
        if (musicClips.Count > 0)
        {
            PlayRandomMusic();
        }
    }

    private void Update()
    {
        // Only perform actions if the current scene is "MenuScene"
        if (SceneManager.GetActiveScene().name == "MenuScene")
        {
            // Detect double-tap input
            DetectDoubleTap();

            // If the slider is not assigned, try to find it
            if (settingsMusicSlider == null)
            {
                FindSlider();
            }

            // Play the next music if the current one has stopped
            if (!audioSource.isPlaying)
            {
                PlayRandomMusic();
            }
        }
    }

    private void DetectDoubleTap()
    {
        // Check for touch or mouse click input
        if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            float currentTime = Time.time;

            // Check if the time difference between two taps is within the threshold
            if (currentTime - lastTapTime <= doubleTapThreshold)
            {
                // Double-tap detected; play random music
                PlayRandomMusic();
            }

            // Update the last tap time
            lastTapTime = currentTime;
        }
    }

    private void FindSlider()
    {
        // Find the Slider under the specified hierarchy
        GameObject canvasObject = GameObject.Find("Canvas");
        if (canvasObject != null)
        {
            Transform settingsScreen = canvasObject.transform.Find("SettingsScreen");
            if (settingsScreen != null)
            {
                Transform sliderTransform = settingsScreen.Find("Slider");
                if (sliderTransform != null)
                {
                    settingsMusicSlider = sliderTransform.GetComponent<Slider>();

                    // If the slider is found, bind its value change event
                    if (settingsMusicSlider != null)
                    {
                        settingsMusicSlider.value = audioSource.volume; // Set initial slider value
                        settingsMusicSlider.onValueChanged.AddListener(SetVolume); // Bind the event
                    }
                }
            }
        }

        if (settingsMusicSlider == null)
        {
            Debug.LogWarning("Slider not found in the specified hierarchy (Canvas > SettingsScreen > Slider).");
        }
    }

    // Play a random AudioClip from the list
    private void PlayRandomMusic()
    {
        if (musicClips.Count == 0) return;

        // Select a random AudioClip
        AudioClip randomClip = musicClips[Random.Range(0, musicClips.Count)];
        audioSource.clip = randomClip;
        audioSource.Play();

        Debug.Log("Playing new song: " + randomClip.name);
    }

    // Get the name of the current AudioClip
    public string GetCurrentSongName()
    {
        return audioSource.clip != null ? audioSource.clip.name : "No Song";
    }

    // Check if a song is playing
    public bool IsPlaying()
    {
        return audioSource.isPlaying;
    }

    // Set volume based on slider value
    private void SetVolume(float value)
    {
        audioSource.volume = value;
    }
}
