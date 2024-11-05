using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("UI Materials")]
    [SerializeField] TextMeshProUGUI usernameText;
    [SerializeField] GameObject usernameScreen;

    [Header("Username UI Materials")]
    [SerializeField] TMP_InputField usernameInputField;
    [SerializeField] Button OkeyButton;

    [Header("Firebase Settings")]
    FirebaseUser user;
    FirebaseFirestore db;
    DocumentReference docRef;

    private void Awake()
    {
        user = FirebaseManager.instance.user;

        db = FirebaseFirestore.DefaultInstance;
        docRef = db.Collection("users").Document(FirebaseManager.instance.user.UserId);

        OkeyButton.onClick.AddListener( () => { CreateUsername(); } );
    }

    private void Start()
    {
        if (FirebaseManager.instance != null && user != null)
        {
            CheckAndCreateUserDocument();
            GetUsername();
        } else
        {
            Debug.LogError("FirebaseManager instance or user is null!");
        }
    }

    // Kullanıcı dökümanını kontrol et ve yoksa oluştur
    public void CheckAndCreateUserDocument()
    {
        if (user != null)
        {
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (!snapshot.Exists)
                    {
                        // Kullanıcı dökümanı yoksa oluştur
                        var userData = new Dictionary<string, object> { { "Username", string.Empty } };

                        docRef.SetAsync(userData).ContinueWithOnMainThread(setTask =>
                        {
                            if (setTask.IsCompleted)
                            {
                                Debug.Log("Kullanıcı dökümanı başarıyla oluşturuldu.");
                                usernameScreen.SetActive(true);
                            }
                            else
                            {
                                Debug.LogError("Kullanıcı dökümanı oluşturulamadı: " + setTask.Exception);
                            }
                        });
                    }else
                    {
                        Debug.Log("Kullanıcı dökümanı zaten var.");
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

    public void CreateUsername()
    {
        string newUsername = usernameInputField.text;

        // İlk olarak users koleksiyonundaki tüm dökümanları al ve kontrol et
        db.Collection("users").GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                bool isUnique = true;
                foreach (DocumentSnapshot document in task.Result.Documents)
                {
                    if (document.TryGetValue("Username", out string existingUsername))
                    {
                        if (existingUsername == newUsername)
                        {
                            isUnique = false;
                            Debug.LogWarning("Bu kullanıcı adı zaten mevcut! Lütfen başka bir kullanıcı adı seçin.");
                            break;
                        }
                    }
                }

                if (isUnique)
                {
                    // Eğer kullanıcı adı benzersizse, kullanıcı dökümanına kaydet
                    var userData = new Dictionary<string, object>
                    {
                        { "Username", newUsername }
                    };

                    docRef.SetAsync(userData, SetOptions.MergeAll).ContinueWithOnMainThread(setTask =>
                    {
                        if (setTask.IsCompleted)
                        {
                            usernameScreen.SetActive(false);
                            Debug.Log("Kullanıcı adı başarıyla kaydedildi!");
                        }
                        else
                        {
                            Debug.LogError("Kullanıcı adı kaydedilirken hata oluştu: " + setTask.Exception);
                        }
                    });
                }
            }
            else
            {
                Debug.LogError("Kullanıcı adlarını kontrol ederken hata oluştu: " + task.Exception);
            }
        });
    }

    public void GetUsername()
    {
        if (FirebaseManager.instance != null && FirebaseManager.instance.user != null)
        {
            FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
            DocumentReference docRef = db.Collection("users").Document(FirebaseManager.instance.user.UserId);

            string username = string.Empty; // Varsayılan boş değer

            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        username = snapshot.GetValue<string>("Username"); // Kullanıcı adını al
                        usernameText.text = username;
                    }
                    else
                    {
                        Debug.LogError("Kullanıcı dökümanı bulunamadı.");
                    }
                }
                else
                {
                    Debug.LogError("Kullanıcı dökümanını alırken hata oluştu: " + task.Exception);
                }
            });

        }
        else
        {
            Debug.LogError("FirebaseManager veya kullanıcı nesnesi null.");
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
