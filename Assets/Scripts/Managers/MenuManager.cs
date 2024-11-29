using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Networking;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

    [SerializeField] private GameObject loadingScreen; // Loading ekraný
    [SerializeField] private TextMeshProUGUI loadingText;

    private string apiUrl = "https://www.healthtourismclinics.com/zl25beta2/kelime/sil.php"; // PHP API URL

    private void Awake()
    {
        // "money" anahtarýnýn var olup olmadýðýný kontrol et
        if (!PlayerPrefs.HasKey("money"))
        {
            // Yoksa, "money" anahtarýyla kaydet
            PlayerPrefs.SetInt("money", 1000);
            PlayerPrefs.Save();
            Debug.Log("Money oluþturuldu ve deðeri 1000 olarak ayarlandý.");
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
        // Kullanýcý adýný al
        string username = PlayerPrefs.GetString("username", "");

        // Kullanýcý adýný sil
        PlayerPrefs.DeleteKey("username");

        // Uygulamanýn kalýcý veri yolu altýndaki tüm .json dosyalarýný sil
        string[] jsonFiles = Directory.GetFiles(Application.persistentDataPath, "*.json");

        foreach (string jsonFile in jsonFiles)
        {
            if (File.Exists(jsonFile))
            {
                File.Delete(jsonFile); // JSON dosyasýný sil
                Debug.Log($"Silindi: {Path.GetFileName(jsonFile)}");
            }
        }

        if (jsonFiles.Length == 0)
        {
            Debug.LogWarning("Hiçbir JSON dosyasý bulunamadý.");
        }
        else
        {
            Debug.Log($"Toplam {jsonFiles.Length} JSON dosyasý silindi.");
        }

        // Kullanýcýyý veritabanýndan sil
        StartCoroutine(DeleteUserFromDatabase(username));

        // Ana sahneye dön
        SceneManager.LoadScene(0);
    }

    private System.Collections.IEnumerator DeleteUserFromDatabase(string username)
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
                Debug.Log("Username baþarýyla veritabanýndan silindi: " + request.downloadHandler.text);
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
        // Loading ekranýný göster
        if (loadingScreen != null)
        {
            loadingScreen.SetActive(true);
        }

        // Sahneyi yüklemeye baþla
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // Yükleme ilerlemesini göster
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // Yüzdelik hesap
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
