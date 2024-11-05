using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public TextMeshProUGUI userMailText;
    public GameObject usernameScreen;

    private void Start()
    {
        if (FirebaseManager.instance != null && FirebaseManager.instance.user != null)
        {
            userMailText.text = FirebaseManager.instance.user.Email;
            Invoke("CheckAndCreateUserDocument", 1f);
        }
        else
        {
            Debug.LogError("FirebaseManager instance or user is null!");
        }
    }

    // Kullanıcı dökümanını kontrol et ve yoksa oluştur
    public void CheckAndCreateUserDocument()
    {
        if (FirebaseManager.instance.user != null)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("users").Document(FirebaseManager.instance.user.UserId);

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (!snapshot.Exists)
                    {
                        // Kullanıcı dökümanı yoksa oluştur
                        var userData = new Dictionary<string, object>
                        {
                            { "username", "Tugi" }, // Başlangıçta boş bir username alanı
                            { "created_at", FieldValue.ServerTimestamp }
                        };

                        docRef.SetAsync(userData).ContinueWithOnMainThread(setTask =>
                        {
                            if (setTask.IsCompleted)
                            {
                                Debug.Log("Kullanıcı dökümanı oluşturuldu.");
                                usernameScreen.SetActive(true); // usernameScreen'i aktif et
                            }
                            else
                            {
                                Debug.LogError("Kullanıcı dökümanı oluşturulamadı: " + setTask.Exception);
                            }
                        });
                    }
                    else
                    {
                        Debug.Log("Kullanıcı dökümanı zaten var.");
                        usernameScreen.SetActive(true); // Döküman zaten varsa yine de ekranı aç
                    }
                }
                else
                {
                    Debug.LogError("Kullanıcı dökümanını kontrol ederken hata oluştu: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("User is null. Cannot check or create document.");
        }
    }

    public void SignOutMethod()
    {
        PlayerPrefs.DeleteKey("RememberMe");
        PlayerPrefs.DeleteKey("SavedEmail");
        PlayerPrefs.DeleteKey("SavedPassword");

        FirebaseManager.instance.SignOut();
        SceneManager.LoadScene(0);
    }
}
