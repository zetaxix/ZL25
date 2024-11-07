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
    FirebaseUser user;
    FirebaseFirestore db;

    [SerializeField] GameObject bronzePackagePrefab;
    [SerializeField] GameObject silverPackagePrefab;
    [SerializeField] GameObject goldPackagePrefab;
    [SerializeField] Transform packagesParent;

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
}