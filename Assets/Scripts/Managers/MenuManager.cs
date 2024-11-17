using System.IO;
using System.Linq.Expressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

    [SerializeField] private GameObject loadingScreen; // Loading ekran�
    [SerializeField] private TextMeshProUGUI loadingText;

    private void Awake()
    {
        // "money" anahtar�n�n var olup olmad���n� kontrol et
        if (!PlayerPrefs.HasKey("money"))
        {
            // Yoksa, "money" anahtar�n� 1000 de�eri ile olu�tur
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
        // Kullan�c� ad�n� sil
        PlayerPrefs.DeleteKey("username");

        // mypackages.json dosyas�n� sil
        string myPackagesFilePath = Path.Combine(Application.persistentDataPath, "mypackages.json");
        string myFootballersFilePath = Path.Combine(Application.persistentDataPath, "myfootballers.json");
        if (File.Exists(myPackagesFilePath) && File.Exists(myFootballersFilePath))
        {
            File.Delete(myPackagesFilePath);
            File.Delete(myFootballersFilePath);

            Debug.Log("mypackages.json ve myfootballers.json dosyalar� silindi, kullan�c� oturumu s�f�rland�.");
        }
        else
        {
            Debug.LogWarning("mypackages.json dosyas� bulunamad�.");
        }

        // Ana sahneye d�n
        SceneManager.LoadScene(0);
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
