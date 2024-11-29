using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

    [SerializeField] private GameObject loadingScreen; // Loading ekran�
    [SerializeField] private TextMeshProUGUI loadingText;

    private string apiUrl = "https://www.healthtourismclinics.com/zl25beta2/kelime/sil.php"; // PHP API URL

    private void Awake()
    {
        // "money" anahtar�n�n var olup olmad���n� kontrol et
        if (!PlayerPrefs.HasKey("money"))
        {
            // Yoksa, "money" anahtar�yla kaydet
            PlayerPrefs.SetInt("money", 1000);
            PlayerPrefs.Save();
            Debug.Log("Money olu�turuldu ve de�eri 1000 olarak ayarland�.");
        }
    }

    private void Start()
    {
        CheckandLoadUsername();
    }

    void CheckandLoadUsername()
    {
        if (PlayerPrefs.HasKey("username"))
        {
            string username = PlayerPrefs.GetString("username");
            usernameText.text = username;
        }
    }

    public void UserSignOutMethod()
    {
        // Kullan�c� ad�n� al
        string username = PlayerPrefs.GetString("username", "");

        // Kullan�c� ad�n� sil
        PlayerPrefs.DeleteKey("username");

        // Uygulaman�n kal�c� veri yolu alt�ndaki t�m .json dosyalar�n� sil
        string[] jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            if (File.Exists(jsonFile))
            {
                File.Delete(jsonFile); // JSON dosyas�n� sil
                Debug.Log($"Silindi: {Path.GetFileName(jsonFile)}");
            }
        }

        if (jsonFiles.Length == 0)
        {
            Debug.LogWarning("Hi�bir JSON dosyas� bulunamad�.");
        }
        else
        {
            Debug.Log($"Toplam {jsonFiles.Length} JSON dosyas� silindi.");
        }

        // Kullan�c�y� veritaban�ndan sil
        StartCoroutine(DeleteUserFromDatabase(username));

        // Ana sahneye d�n
        SceneManager.LoadScene(0);
    }

    private System.Collections.IEnumerator DeleteUserFromDatabase(string username)
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
                Debug.Log("Username ba�ar�yla veritaban�ndan silindi: " + request.downloadHandler.text);
            }
            else
            {
                Debug.LogError("Error deleting username: " + request.error);
            }
        }
    }

    public void GoToShopScene()
    {
        StartCoroutine(LoadSceneAsync("ShopScene"));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName)
    {
        // Loading ekran�n� g�ster
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // Sahneyi y�klemeye ba�la
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Y�kleme ilerlemesini g�ster
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Y�zdelik hesap
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {progress * 100:F0}%";
            }

            yield return null; // Bir sonraki frame'i bekle
        }
    }

    public void GoToFootballersScene()
    {
        SceneManager.LoadScene(3);
    }

    public void GotoGameMode()
    {
        SceneManager.LoadScene(4);
    }
}
