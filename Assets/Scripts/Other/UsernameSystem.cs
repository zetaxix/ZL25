using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class UsernameSystem : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField; // Username'i girece�iniz InputField
    [SerializeField] private string apiUrl = "https://www.healthtourismclinics.com/zl25beta2/kelime/kullanici.php"; // PHP API URL

    private void Awake()
    {
        // Kullan�c� ad�nda de�i�iklik oldu�unda kontrol eden dinleyici ekle
        usernameInputField.onValueChanged.AddListener(ValidateUsernameInput);
        HasUsername();
    }

    public void CheckAndSaveUsername()
    {
        // "username" anahtar� olup olmad���n� kontrol et
        if (!PlayerPrefs.HasKey("username"))
        {
            string username = usernameInputField.text;

            // Kullan�c� ad� kurallar�n� kontrol et
            if (!string.IsNullOrEmpty(username))
            {
                // "username" anahtar�yla kaydet
                PlayerPrefs.SetString("username", username);
                PlayerPrefs.Save(); // De�i�iklikleri kaydet
                Debug.Log("Username Giri� yap�ld�: " + username);

                // API'ye kullan�c� ad�n� g�nder
                StartCoroutine(SendUsernameToServer(username));

                // Sahneye ge�
                SceneManager.LoadScene(1);
            }
            else
            {
                Debug.LogWarning("Kullan�c� ad� bo� b�rak�lamaz.");
            }
        }
        else
        {
            Debug.Log("Username zaten kay�tl�: " + PlayerPrefs.GetString("username"));
        }
    }

    private void ValidateUsernameInput(string input)
    {
        // Maksimum 8 karakteri a�arsa k�salt
        if (input.Length > 8)
        {
            usernameInputField.text = input.Substring(0, 8);
        }

        // T�rk�e karakterleri temizle
        string sanitizedInput = RemoveTurkishCharacters(input);
        if (sanitizedInput != input)
        {
            usernameInputField.text = sanitizedInput;
        }
    }

    private string RemoveTurkishCharacters(string input)
    {
        string turkishChars = "������������";
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

    // Kullan�c� ad�n� PHP sunucusuna g�nderme i�lemi
    IEnumerator SendUsernameToServer(string username)
    {
        // JSON verisini olu�tur
        string jsonData = "{\"username\":\"" + username + "\"}";

        // POST iste�i g�nder
        using (UnityWebRequest request = UnityWebRequest.Put(apiUrl, jsonData))
        {
            request.method = UnityWebRequest.kHttpVerbPOST; // POST iste�i olarak ayarl�yoruz
            request.SetRequestHeader("Content-Type", "application/json");

            // Veriyi g�nder
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Username ba�ar�yla g�nderildi: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error sending username: " + request.error);
            }
        }
    }
}
