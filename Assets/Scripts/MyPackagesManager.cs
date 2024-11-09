using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyPackagesManager : MonoBehaviour
{
    [Header("Footballer Settings")]
    public List<Footballer> bronzeFootballers = new List<Footballer>();
    public List<Footballer> silverFootballers = new List<Footballer>();
    public List<Footballer> goldFootballers = new List<Footballer>();

    [SerializeField] GameObject playerCardPrefab;
    [SerializeField] Transform playerCardParent; // Kart�n eklenece�i parent

    FirebaseUser user;
    FirebaseFirestore db;

    [Header("Packages Settings")]
    [SerializeField] GameObject bronzePackagePrefab;
    [SerializeField] GameObject silverPackagePrefab;
    [SerializeField] GameObject goldPackagePrefab;
    [SerializeField] Transform packagesParent;

    [SerializeField] GameObject packageScreen;

    private Dictionary<string, GameObject> packageObjects = new Dictionary<string, GameObject>();

    private void Start()
    {
        user = FirebaseManager.instance.user;
        db = FirebaseFirestore.DefaultInstance;

        LoadUserPackages();
    }

    public void LoadUserPackages()
    {
        if (user == null)
        {
            Debug.LogError("User is null. Cannot load packages.");
            return;
        }

        DocumentReference docRef = db.Collection("users").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;

                UpdateOrCreatePackageUI("Bronze Package", snapshot, bronzePackagePrefab);
                UpdateOrCreatePackageUI("Silver Package", snapshot, silverPackagePrefab);
                UpdateOrCreatePackageUI("Gold Package", snapshot, goldPackagePrefab);
            }
            else
            {
                Debug.LogError("Paketler y�klenirken hata olu�tu: " + task.Exception);
            }
        });
    }

    private void UpdateOrCreatePackageUI(string packageName, DocumentSnapshot snapshot, GameObject packagePrefab)
    {
        if (snapshot.ContainsField(packageName))
        {
            var packageData = snapshot.GetValue<Dictionary<string, object>>(packageName);
            int count = Convert.ToInt32(packageData["count"]);

            if (count > 0)
            {
                if (packageObjects.ContainsKey(packageName))
                {
                    // Paket zaten varsa, sadece say�y� g�ncelle
                    TextMeshProUGUI countText = packageObjects[packageName].transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                    countText.text = $"x{count}";
                }
                else
                {
                    // Paket yoksa, yeni olu�tur
                    GameObject packageObject = Instantiate(packagePrefab, packagesParent);
                    TextMeshProUGUI countText = packageObject.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                    countText.text = $"x{count}";

                    // Butona t�klama i�lemi ekle ve count'u azalt
                    Button openButton = packageObject.transform.Find("Button").GetComponent<Button>();
                    openButton.onClick.AddListener(() =>
                    {
                        if (count > 0)
                        {
                            count--;
                            countText.text = $"x{count}";
                            Debug.Log($"{packageName} a��ld�!");

                            OpenPackage(packageName);

                            // Firebase Firestore'da g�ncelleme
                            DocumentReference docRef = db.Collection("users").Document(user.UserId);
                            Dictionary<string, object> updates = new Dictionary<string, object>
                            {
                                { $"{packageName}.count", count }
                            };
                            docRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompleted)
                                {
                                    Debug.Log($"{packageName} g�ncellendi. Kalan: {count}");

                                    // E�er count s�f�rsa UI'dan kald�r
                                    if (count == 0)
                                    {
                                        Destroy(packageObject);
                                        packageObjects.Remove(packageName);
                                    }
                                }
                                else
                                {
                                    Debug.LogError($"{packageName} g�ncellenemedi: " + updateTask.Exception);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log($"{packageName} t�kendi!");
                        }
                    });

                    // Olu�turulan paketi kaydet
                    packageObjects[packageName] = packageObject;
                }
            }
        }
    }

    public void OpenPackage(string packageType)
    {
        packageScreen.SetActive(true);

        List<Footballer> selectedFootballers = null;

        // Paket t�r�ne g�re oyuncu listesini se�
        if (packageType == "Bronze Package")
            selectedFootballers = bronzeFootballers;
        else if (packageType == "Silver Package")
            selectedFootballers = silverFootballers;
        else if (packageType == "Gold Package")
            selectedFootballers = goldFootballers;

        if (selectedFootballers != null && selectedFootballers.Count > 0)
        {
            Footballer randomFootballer = selectedFootballers[UnityEngine.Random.Range(0, selectedFootballers.Count)];
            ShowPlayerCard(randomFootballer);
        }
        else
        {
            Debug.Log("Bu paket t�r�nde oyuncu bulunamad�.");
        }
    }

    private void ShowPlayerCard(Footballer footballer)
    {
        GameObject card = Instantiate(playerCardPrefab, playerCardParent);

        // Karttaki UI elemanlar�n� bulup g�ncelle
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();
        RawImage footballerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();

        //Footballer resim verisini Texture2D ile e�le�tir
        Texture2D countryFlagTexture = footballer.countryFlag;
        Texture2D teamLogoTexture = footballer.teamLogo;
        Texture2D footballerImageTexture = footballer.footballerImage;

        // UI elemanlar�n� footballer bilgileriyle g�ncelle
        nameText.text = footballer.name;
        ratingText.text = $"{footballer.rating}";
        priceText.text = $"Price: �{footballer.price}"; // �rne�in: Price: $1000 �eklinde g�ster

        countryFlagImage.texture = countryFlagTexture;
        teamLogoImage.texture = teamLogoTexture;
        footballerImage.texture = footballerImageTexture;

        SaveFootballerToFirebase(footballer);
    }

    private void SaveFootballerToFirebase(Footballer footballer)
    {
        // Kullan�c�n�n d�k�man�na eri�
        DocumentReference userDoc = db.Collection("users").Document(user.UserId);

        // `footballer.[oyuncu ad�].count` de�erini kontrol et
        userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                // Mevcut say�y� kontrol etmek i�in de�i�ken
                int currentCount = 0;

                // E�er oyuncu daha �nce kaydedilmi�se `count` de�eri al�n�r
                if (snapshot.ContainsField($"footballer.{footballer.name}.count"))
                {
                    currentCount = snapshot.GetValue<int>($"footballer.{footballer.name}.count");
                }

                // Yeni oyuncu verilerini olu�tur
                Dictionary<string, object> playerData = new Dictionary<string, object>
                {
                    { "id", footballer.id },
                    { "rating", footballer.rating },
                    { "price", footballer.price },
                    { "count", currentCount + 1 } // Mevcut count �zerine 1 eklenir
                };

                // `footballer` map'ine bu oyuncuyu ekle veya g�ncelle
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { $"footballer.{footballer.name}", playerData }
                };

                userDoc.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log($"{footballer.name} bilgileri g�ncellendi, count: {currentCount + 1}");
                    }
                    else
                    {
                        Debug.LogError("Footballer bilgileri g�ncellenemedi: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Kullan�c� d�k�man� al�namad�: " + task.Exception);
            }
        });
    }


}