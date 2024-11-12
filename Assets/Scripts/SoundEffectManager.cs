using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip fadeSoundEffect; // Ses efekti dosyas�
    public AudioClip fireworkSoundEffect; // Ses efekti dosyas�
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bile�enini ekleyin veya referans al�n
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // Animation Event'in �a��raca�� ses oynatma fonksiyonu
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
