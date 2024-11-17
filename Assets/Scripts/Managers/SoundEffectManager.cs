using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip fadeSoundEffect; // Ses efekti dosyasý
    public AudioClip fireworkSoundEffect; // Ses efekti dosyasý
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bileþenini ekleyin veya referans alýn
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Animation Event'in çaðýracaðý ses oynatma fonksiyonu
    public void PlaySoundEffect()
    {
        if (fadeSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(fadeSoundEffect);
        }
    }

    public void PlayFireworkSound()
    {
        if (fireworkSoundEffect != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireworkSoundEffect);
        }
    }
}
