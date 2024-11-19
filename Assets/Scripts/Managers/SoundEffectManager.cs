using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip sound; // Ses efekti dosyasý
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bileþenini ekleyin veya referans alýn
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        //PlaySoundEffect();
    }

    // Animation Event'in çaðýracaðý ses oynatma fonksiyonu
    public void PlaySoundEffect()
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
}
