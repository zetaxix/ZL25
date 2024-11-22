using System.IO;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

    [SerializeField] private GameObject loadingScreen; // Loading ekraný
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        // "money" anahtarýnýn var olup olmadýðýný kontrol et
        if (!PlayerPrefs.HasKey("money"))
        {
            // Yoksa, "money" anahtarýný 1000 deðeri ile oluþtur
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
        // Kullanýcý adýný sil
        PlayerPrefs.DeleteKey("username");

        // Uygulamanýn kalýcý veri yolu altýndaki tüm .json dosyalarýný bul
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

        // Ana sahneye dön
        SceneManager.LoadScene(0);
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
