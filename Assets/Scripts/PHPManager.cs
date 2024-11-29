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
    public List<User> users = new List<User>(); // Kullan�c� listesi, di�er s�n�flar buradan eri�ebilir

    [SerializeField] RawImage ShowPhpImage; // RawImage bile�eni

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

                // Kullan�c�lar�n profil resimlerini �ek
                if (users.Count > 0)
                {
                    // Burada ilk kullan�c�n�n profil resim linkini al�yoruz
                    string imageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTztdJPLRKO3U2KCyg3B8Lx-KSBrpziMKPKig&s";
                    Debug.Log("Resim URL: " + imageUrl);

                    // Resmi indirmeye ba�l�yoruz
                    StartCoroutine(DownloadImage(imageUrl));
                }
            }
            else
            {
                Debug.LogError("Error fetching users: " + request.error);
            }
        }
    }

    // Resmi indirme i�lemi
    IEnumerator DownloadImage(string imageUrl)
    {
        // URL'nin do�ru olup olmad���n� kontrol et
        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogError("URL bo� veya ge�ersiz: " + imageUrl);
            yield break;
        }

        // URL'yi do�ru bir �ekilde kullanarak resim indirmeye ba�la
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            // Resmi indirmeye ba�la
            yield return request.SendWebRequest();

            // Hata kontrol�
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Resim ba�ar�yla indirildi
                Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

                // RawImage bile�enine bu resmi at�yoruz
                ShowPhpImage.texture = texture;
            }
            else
            {
                // Hata durumunda log mesaj� ver
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
