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
    [SerializeField] Transform packagesParent; // Paketleri eklemek için kullanýlan parent obje

    private void Start()
    {
        user = FirebaseManager.instance.user;
        db = FirebaseFirestore.DefaultInstance;

        LoadUserPackages();
    }

    private void LoadUserPackages()
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

                // Bronze Package varsa göster
                if (snapshot.ContainsField("Bronze Package"))
                {
                    var bronzeData = snapshot.GetValue<Dictionary<string, object>>("Bronze Package");
                    int count = Convert.ToInt32(bronzeData["count"]);
                    CreatePackageUI(bronzePackagePrefab, count);
                }

                // Silver Package varsa göster
                if (snapshot.ContainsField("Silver Package"))
                {
                    var silverData = snapshot.GetValue<Dictionary<string, object>>("Silver Package");
                    int count = Convert.ToInt32(silverData["count"]);
                    CreatePackageUI(silverPackagePrefab, count);
                }

                // Gold Package varsa göster
                if (snapshot.ContainsField("Gold Package"))
                {
                    var goldData = snapshot.GetValue<Dictionary<string, object>>("Gold Package");
                    int count = Convert.ToInt32(goldData["count"]);
                    CreatePackageUI(goldPackagePrefab, count);
                }
            }
            else
            {
                Debug.LogError("Paketler yüklenirken hata oluþtu: " + task.Exception);
            }
        });
    }

    private void CreatePackageUI(GameObject packagePrefab, int count)
    {
        // Paketi UI'ya ekle
        GameObject packageObject = Instantiate(packagePrefab, packagesParent);

        // Paket sayýsýna göre burada diðer özelleþtirmeleri yapabilirsiniz
    }
}
