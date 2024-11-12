using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MyPackages : MonoBehaviour
{
    [SerializeField] Button myPackageButton;
    [SerializeField] GameObject packageParentObject; // Paketlerin gösterileceði alan (GridLayout içinde)
    [SerializeField] GameObject bronzePackagePrefab;  // Bronze paketi prefab'ý
    [SerializeField] GameObject silverPackagePrefab;  // Silver paketi prefab'ý
    [SerializeField] GameObject goldPackagePrefab;    // Gold paketi prefab'ý

    [SerializeField] GameObject cardScreen;
    [SerializeField] GameObject packageScreen;

    private string jsonFilePath;

    [SerializeField] CardManager cardManager; // CardManager referansý

    private void Awake()
    {
        myPackageButton.onClick.AddListener(() => { ShowUserPackages(); });
        jsonFilePath = Path.Combine(Application.persistentDataPath, "mypackages.json");  // Dosya yolunu belirle
    }

    void ShowUserPackages()
    {
        // Önceden eklenmiþ paketleri temizle
        foreach (Transform child in packageParentObject.transform)
        {
            Destroy(child.gameObject);
        }

        // JSON dosyasýný oku ve paketleri yükle
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);

            // JSON verisini Dictionary þeklinde deserialize et
            var packageData = JsonUtility.FromJson<PackagesDictionary>(json);

            // Eðer paket verisi varsa
            if (packageData != null)
            {
                // Paketleri kontrol et ve prefablarý ekle
                foreach (var package in packageData.packages)
                {
                    if (package.count > 0) // Paket sayýsý 0'dan büyükse
                    {
                        GameObject packagePrefab = null;

                        // Paket adýna göre prefab seç
                        if (package.name == "Bronze")
                        {
                            packagePrefab = bronzePackagePrefab;
                        }
                        else if (package.name == "Silver")
                        {
                            packagePrefab = silverPackagePrefab;
                        }
                        else if (package.name == "Gold")
                        {
                            packagePrefab = goldPackagePrefab;
                        }

                        // Eðer prefab bulunduysa, sadece 1 tane instantiation yap
                        if (packagePrefab != null)
                        {
                            GameObject instantiatedPackage = Instantiate(packagePrefab, packageParentObject.transform);

                            // PackagePrefab'daki "countText" TextMeshProUGUI bileþenini bul
                            TextMeshProUGUI countText = instantiatedPackage.GetComponentInChildren<TextMeshProUGUI>();

                            if (countText != null)
                            {
                                // countText'e count deðerini ata
                                countText.text = $"x{package.count.ToString()}";
                            }
                            else
                            {
                                Debug.LogWarning("Prefab içerisinde 'countText' adlý TextMeshProUGUI bileþeni bulunamadý.");
                            }

                            // Paket içindeki butona referans al
                            Button packageButton = instantiatedPackage.transform.Find("Button")?.GetComponent<Button>();

                            if (packageButton != null)
                            {
                                // Butona týklama event'i ekle
                                packageButton.onClick.AddListener(() =>
                                {
                                    // Paket sayýsýný 1 eksilt
                                    if (package.count > 0)
                                    {
                                        package.count--;
                                        // TextMeshPro içerisine yeni sayýyý ata
                                        countText.text = $"x{package.count.ToString()}";
                                        Debug.Log($"{package.name} paketi sayýsý: {package.count}");

                                        packageScreen.SetActive(false);
                                        cardScreen.SetActive(true);

                                        // Hangi paketin açýldýðýný CardManager'a gönder
                                        cardManager.SetPackageType(package.name);

                                        // Sayý sýfýr olduðunda butonu devre dýþý býrak
                                        if (package.count == 0)
                                        {
                                            packageButton.interactable = false;
                                        }

                                        // JSON dosyasýný güncelle ve kaydet
                                        SaveUpdatedPackageData(packageData);
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Paket JSON dosyasý bulunamadý.");
        }
    }

    void SaveUpdatedPackageData(PackagesDictionary packageData)
    {
        // Güncellenmiþ paket verisini JSON formatýnda serialize et
        string updatedJson = JsonUtility.ToJson(packageData, true);

        // JSON verisini dosyaya kaydet
        File.WriteAllText(jsonFilePath, updatedJson);

        Debug.Log("Paket verisi kaydedildi.");
    }

}

// Paket verisinin JSON formatýna uygun olarak deserialize edilebilmesi için
[System.Serializable]
public class PackagesDictionary
{
    public List<PackageData> packages;
}
