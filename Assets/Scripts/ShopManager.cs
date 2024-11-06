using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    FirebaseUser user;
    FirebaseFirestore db;

    [SerializeField] TextMeshProUGUI moneyText;

    private void Awake()
    {
        // Firebase kullanýcý ve Firestore baðlantýlarýný ayarla
        user = FirebaseManager.instance.user;
        db = FirebaseFirestore.DefaultInstance;
    }

    private void Start()
    {
        GetUserMoney();
    }

    public void BuyBronzePackage()
    {
        PurchasePackage("Bronze Package", 20);
    }

    public void BuySilverPackage()
    {
        PurchasePackage("Silver Package", 40);
    }

    public void BuyGoldPackage()
    {
        PurchasePackage("Gold Package", 100);
    }

    private bool isUpdatingMoney = false; // Bu flag güncelleme sýrasýnda yeni sorgulamalarý engellemek için

    //Paket Satýn Alma Fonksiyonu
    public void PurchasePackage(string packageName, int price)
    {
        if (user != null)
        {
            DocumentReference docRef = db.Collection("users").Document(user.UserId);

            // Kullanýcýnýn mevcut parasýný kontrol et ve paketi satýn alacak kadar parasý olup olmadýðýný kontrol et
            docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCompleted && task.Result.Exists)
                {
                    DocumentSnapshot snapshot = task.Result;

                    // Kullanýcýnýn money deðeri mevcutsa çek
                    if (snapshot.ContainsField("Money"))
                    {
                        long money = snapshot.GetValue<long>("Money");

                        if (money >= price)
                        {
                            long newMoney = money - price;

                            Dictionary<string, object> packageData = new Dictionary<string, object>();
                            if (snapshot.ContainsField(packageName))
                            {
                                packageData = snapshot.GetValue<Dictionary<string, object>>(packageName);
                                int currentCount = packageData.ContainsKey("count") ? Convert.ToInt32(packageData["count"]) : 0;
                                packageData["count"] = currentCount + 1;
                            }
                            else
                            {
                                packageData["count"] = 1;
                            }

                            Dictionary<string, object> updates = new Dictionary<string, object>
                        {
                            { "Money", newMoney },
                            { packageName, packageData }
                        };

                            isUpdatingMoney = true;

                            docRef.UpdateAsync(updates).ContinueWithOnMainThread(updateTask =>
                            {
                                if (updateTask.IsCompleted)
                                {
                                    Debug.Log($"{packageName} baþarýyla satýn alýndý ve kaydedildi. Yeni bakiye: {newMoney}");
                                    moneyText.text = $"Money: €{newMoney:N0}";
                                }
                                else
                                {
                                    Debug.LogError("Güncelleme sýrasýnda hata oluþtu: " + updateTask.Exception);
                                }

                                isUpdatingMoney = false;
                            });
                        }
                        else
                        {
                            Debug.LogWarning("Yeterli bakiye yok.");
                            moneyText.text = "Yeterli bakiye yok!";
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Money bilgisi bulunamadý.");
                    }
                }
                else
                {
                    Debug.LogError("Kullanýcý dokümanýný alýrken hata oluþtu: " + task.Exception);
                }
            });
        }
        else
        {
            Debug.LogError("User is null. Cannot purchase package.");
        }
    }

    //Kullanýcý Money Verisi Çekme Fonksiyonu
    void GetUserMoney()
    {
        if (isUpdatingMoney || user == null) return; // Güncelleme sýrasýnda yeni bir çekim yapýlmasýný engelle

        DocumentReference docRef = db.Collection("users").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && task.Result.Exists)
            {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.ContainsField("Money"))
                {
                    long money = snapshot.GetValue<long>("Money");
                    moneyText.text = $"Money: €{money:N0}";
                }
                else
                {
                    moneyText.text = "Money: €0";
                }
            }
            else
            {
                Debug.LogError("Kullanýcý dokümanýný alýrken hata oluþtu: " + task.Exception);
                moneyText.text = "Money: €0";
            }
        });
    }

    //Menüye Dönme
    public void BackToMenu()
    {
        SceneManager.LoadScene(1);
    }

}
