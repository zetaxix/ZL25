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
    [SerializeField] Transform playerCardParent; // Kartýn ekleneceði parent

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
                Debug.LogError("Paketler yüklenirken hata oluþtu: " + task.Exception);
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
                    // Paket zaten varsa, sadece sayýyý güncelle
                    TextMeshProUGUI countText = packageObjects[packageName].transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                    countText.text = $"x{count}";
                }
                else
                {
                    // Paket yoksa, yeni oluþtur
                    GameObject packageObject = Instantiate(packagePrefab, packagesParent);
                    TextMeshProUGUI countText = packageObject.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
                    countText.text = $"x{count}";

                    // Butona týklama iþlemi ekle ve count'u azalt
                    Button openButton = packageObject.transform.Find("Button").GetComponent<Button>();
                    openButton.onClick.AddListener(() =>
                    {
                        if (count > 0)
                        {
                            count--;
                            countText.text = $"x{count}";
                            Debug.Log($"{packageName} açýldý!");

                            OpenPackage(packageName);

                            // Firebase Firestore'da güncelleme
                            DocumentReference docRef = db.Collection("users").Document(user.UserId);
                            Dictionary<string, object> updates = new Dictionary<string, object>
                            {
                                { $"{packageName}.count", count }
                            };
                            docRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompleted)
                                {
                                    Debug.Log($"{packageName} güncellendi. Kalan: {count}");

                                    // Eðer count sýfýrsa UI'dan kaldýr
                                    if (count == 0)
                                    {
                                        Destroy(packageObject);
                                        packageObjects.Remove(packageName);
                                    }
                                }
                                else
                                {
                                    Debug.LogError($"{packageName} güncellenemedi: " + updateTask.Exception);
                                }
                            });
                        }
                        else
                        {
                            Debug.Log($"{packageName} tükendi!");
                        }
                    });

                    // Oluþturulan paketi kaydet
                    packageObjects[packageName] = packageObject;
                }
            }
        }
    }

    public void OpenPackage(string packageType)
    {
        packageScreen.SetActive(true);

        List<Footballer> selectedFootballers = null;

        // Paket türüne göre oyuncu listesini seç
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
            Debug.Log("Bu paket türünde oyuncu bulunamadý.");
        }
    }

    private void ShowPlayerCard(Footballer footballer)
    {
        GameObject card = Instantiate(playerCardPrefab, playerCardParent);

        // Karttaki UI elemanlarýný bulup güncelle
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();
        RawImage footballerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();

        //Footballer resim verisini Texture2D ile eþleþtir
        Texture2D countryFlagTexture = footballer.countryFlag;
        Texture2D teamLogoTexture = footballer.teamLogo;
        Texture2D footballerImageTexture = footballer.footballerImage;

        // UI elemanlarýný footballer bilgileriyle güncelle
        nameText.text = footballer.name;
        ratingText.text = $"{footballer.rating}";
        priceText.text = $"Price: €{footballer.price}"; // Örneðin: Price: $1000 þeklinde göster

        countryFlagImage.texture = countryFlagTexture;
        teamLogoImage.texture = teamLogoTexture;
        footballerImage.texture = footballerImageTexture;

        SaveFootballerToFirebase(footballer);
    }

    private void SaveFootballerToFirebase(Footballer footballer)
    {
        // Kullanýcýnýn dökümanýna eriþ
        DocumentReference userDoc = db.Collection("users").Document(user.UserId);

        // `footballer.[oyuncu adý].count` deðerini kontrol et
        userDoc.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;

                // Mevcut sayýyý kontrol etmek için deðiþken
                int currentCount = 0;

                // Eðer oyuncu daha önce kaydedilmiþse `count` deðeri alýnýr
                if (snapshot.ContainsField($"footballer.{footballer.name}.count"))
                {
                    currentCount = snapshot.GetValue<int>($"footballer.{footballer.name}.count");
                }

                // Yeni oyuncu verilerini oluþtur
                Dictionary<string, object> playerData = new Dictionary<string, object>
                {
                    { "id", footballer.id },
                    { "rating", footballer.rating },
                    { "price", footballer.price },
                    { "count", currentCount + 1 } // Mevcut count üzerine 1 eklenir
                };

                // `footballer` map'ine bu oyuncuyu ekle veya güncelle
                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { $"footballer.{footballer.name}", playerData }
                };

                userDoc.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                {
                    if (updateTask.IsCompleted)
                    {
                        Debug.Log($"{footballer.name} bilgileri güncellendi, count: {currentCount + 1}");
                    }
                    else
                    {
                        Debug.LogError("Footballer bilgileri güncellenemedi: " + updateTask.Exception);
                    }
                });
            }
            else
            {
                Debug.LogError("Kullanýcý dökümaný alýnamadý: " + task.Exception);
            }
        });
    }


}