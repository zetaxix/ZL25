using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

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

        // mypackages.json dosyasýný sil
        string filePath = Path.Combine(Application.persistentDataPath, "mypackages.json");
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log("mypackages.json dosyasý silindi.");
        }
        else
        {
            Debug.LogWarning("mypackages.json dosyasý bulunamadý.");
        }

        // Ana sahneye dön
        SceneManager.LoadScene(0);
    }

    public void GoToShopScene()
    {
        SceneManager.LoadScene(2);
    }
}
