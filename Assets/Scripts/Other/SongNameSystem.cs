using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro için gerekli

public class SongNameSystem : MonoBehaviour
{
    // Reference to the TextMeshProUGUI for displaying the song name
    private TextMeshProUGUI songText;

    // Reference to the Animator for triggering animations
    private Animator animator;

    private void Start()
    {
        // Initialize songText and animator
        songText = GetComponentInChildren<TextMeshProUGUI>();
        animator = GetComponent<Animator>();

        if (songText == null)
        {
            Debug.LogWarning("TextMeshProUGUI component not found in children.");
        }

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on SongName object.");
        }
    }

    private void Update()
    {
        // Access the current playing song name from MenuMusicManager
        if (MenuMusicManager.Instance != null && MenuMusicManager.Instance.IsPlaying())
        {
            string currentSongName = MenuMusicManager.Instance.GetCurrentSongName();

            if (songText != null && currentSongName != songText.text)
            {
                // Update the songText UI with the current song name
                songText.text = currentSongName;

                // Trigger animation
                if (animator != null)
                {
                    animator.SetTrigger("SongAnim");
                }
            }
        }
    }
}
