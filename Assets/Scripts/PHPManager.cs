using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;

[System.Serializable]
public class User
{
    public int id;
    public string username;
    public string profileImage; // PHP'den gelen resim linki
}

public class PHPManager : MonoBehaviour
{
    [SerializeField] private string apiUrl = "https://www.healthtourismclinics.com/zl25beta2/kelime/users.php";
    public List<User> users = new List<User>(); // Kullanýcý listesi, diðer sýnýflar buradan eriþebilir

    [SerializeField] RawImage ShowPhpImage; // RawImage bileþeni

    private void Awake()
    {
        StartCoroutine(FetchUsers());
    }

    IEnumerator FetchUsers()
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string json = request.downloadHandler.text;
                users = new List<User>(JsonHelper.FromJson<User>(json));

                // Kullanýcýlarýn profil resimlerini çek
                if (users.Count > 0)
                {
                    // Burada ilk kullanýcýnýn profil resim linkini alýyoruz
                    string imageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTztdJPLRKO3U2KCyg3B8Lx-KSBrpziMKPKig&s";
                    Debug.Log("Resim URL: " + imageUrl);

                    // Resmi indirmeye baþlýyoruz
                    StartCoroutine(DownloadImage(imageUrl));
                }
            }
            else
            {
                Debug.LogError("Error fetching users: " + request.error);
            }
        }
    }

    // Resmi indirme iþlemi
    IEnumerator DownloadImage(string imageUrl)
    {
        // URL'nin doðru olup olmadýðýný kontrol et
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("URL boþ veya geçersiz: " + imageUrl);
            yield break;
        }

        // URL'yi doðru bir þekilde kullanarak resim indirmeye baþla
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            // Resmi indirmeye baþla
            yield return request.SendWebRequest();

            // Hata kontrolü
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Resim baþarýyla indirildi
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                // RawImage bileþenine bu resmi atýyoruz
                ShowPhpImage.texture = texture;
            }
            else
            {
                // Hata durumunda log mesajý ver
                Debug.LogError("Error downloading image: " + request.error);
            }
        }
    }


}

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string newJson = "{ \"array\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.array;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
