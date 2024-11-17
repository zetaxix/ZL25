using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UsernameSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField; // Username'i gireceðiniz InputField

    private void Awake()
    {
        HasUsername();
    }

    public void CheckAndSaveUsername()
    {
        // "username" anahtarý olup olmadýðýný kontrol et
        if (!PlayerPrefs.HasKey("username"))
        {
            string username = usernameInputField.text;

            if (!string.IsNullOrEmpty(username))
            {
                // "username" anahtarýyla kaydet
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save(); // Deðiþiklikleri kaydet
                Debug.Log("Username Giriþ yapýldý: " + username);

                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogWarning("Username alaný boþ býrakýlamaz.");
            }
        }
        else
        {
            Debug.Log("Username zaten kayýtlý: " + PlayerPrefs.GetString("username"));
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
