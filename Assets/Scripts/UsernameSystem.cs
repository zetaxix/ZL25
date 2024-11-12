using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UsernameSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField; // Username'i girece�iniz InputField

    private void Awake()
    {
        HasUsername();
    }

    public void CheckAndSaveUsername()
    {
        // "username" anahtar� olup olmad���n� kontrol et
        if (!PlayerPrefs.HasKey("username"))
        {
            string username = usernameInputField.text;

            if (!string.IsNullOrEmpty(username))
            {
                // "username" anahtar�yla kaydet
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save(); // De�i�iklikleri kaydet
                Debug.Log("Username Giri� yap�ld�: " + username);

                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogWarning("Username alan� bo� b�rak�lamaz.");
            }
        }
        else
        {
            Debug.Log("Username zaten kay�tl�: " + PlayerPrefs.GetString("username"));
        }
    }

    void HasUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            SceneManager.LoadScene(1);
        }
    }
}
