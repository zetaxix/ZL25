using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI usernameText;

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
        SceneManager.LoadScene(2);
    }

    public void GoToFootballersScene()
    {
        SceneManager.LoadScene(3);
    }
}
