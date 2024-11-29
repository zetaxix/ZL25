using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class UsernameSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField; // Username'i gireceðiniz InputField
    [SerializeField] private string apiUrl = "https://www.healthtourismclinics.com/zl25beta2/kelime/kullanici.php"; // PHP API URL

    private void Awake()
    {
        // Kullanýcý adýnda deðiþiklik olduðunda kontrol eden dinleyici ekle
        usernameInputField.onValueChanged.AddListener(ValidateUsernameInput);
        HasUsername();
    }

    public void CheckAndSaveUsername()
    {
        // "username" anahtarý olup olmadýðýný kontrol et
        if (!PlayerPrefs.HasKey("username"))
        {
            string username = usernameInputField.text;

            // Kullanýcý adý kurallarýný kontrol et
            if (!string.IsNullOrEmpty(username))
            {
                // "username" anahtarýyla kaydet
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save(); // Deðiþiklikleri kaydet
                Debug.Log("Username Giriþ yapýldý: " + username);

                // API'ye kullanýcý adýný gönder
                StartCoroutine(SendUsernameToServer(username));

                // Sahneye geç
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogWarning("Kullanýcý adý boþ býrakýlamaz.");
            }
        }
        else
        {
            Debug.Log("Username zaten kayýtlý: " + PlayerPrefs.GetString("username"));
        }
    }

    private void ValidateUsernameInput(string input)
    {
        // Maksimum 8 karakteri aþarsa kýsalt
        if (input.Length > 8)
        {
            usernameInputField.text = input.Substring(0, 8);
        }

        // Türkçe karakterleri temizle
        string sanitizedInput = RemoveTurkishCharacters(input);
        if (sanitizedInput != input)
        {
            usernameInputField.text = sanitizedInput;
        }
    }

    private string RemoveTurkishCharacters(string input)
    {
        string turkishChars = "çðýöþüÇÐÝÖÞÜ";
        string replacementChars = "cgiosuCGIOSU";

        char[] result = input.ToCharArray();
        for (int i = 0; i < input.Length; i++)
        {
            int index = turkishChars.IndexOf(result[i]);
            if (index != -1)
            {
                result[i] = replacementChars[index];
            }
        }

        return new string(result);
    }

    void HasUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            SceneManager.LoadScene(1);
        }
    }

    // Kullanýcý adýný PHP sunucusuna gönderme iþlemi
    IEnumerator SendUsernameToServer(string username)
    {
        // JSON verisini oluþtur
        string jsonData = "{\"username\":\"" + username + "\"}";

        // POST isteði gönder
        using (UnityWebRequest request = UnityWebRequest.Put(apiUrl, jsonData))
        {
            request.method = UnityWebRequest.kHttpVerbPOST; // POST isteði olarak ayarlýyoruz
            request.SetRequestHeader("Content-Type", "application/json");

            // Veriyi gönder
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Username baþarýyla gönderildi: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error sending username: " + request.error);
            }
        }
    }
}
