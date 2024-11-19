using UnityEngine;

public class SoundEffectManager : MonoBehaviour
{
    public AudioClip sound; // Ses efekti dosyas�
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bile�enini ekleyin veya referans al�n
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        //PlaySoundEffect();
    }

    // Animation Event'in �a��raca�� ses oynatma fonksiyonu
    public void PlaySoundEffect()
    {
        if (sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }
}
