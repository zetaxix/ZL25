using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class MyPackages : MonoBehaviour
{
    [SerializeField] Button myPackageButton;
    [SerializeField] GameObject packageParentObject; // Paketlerin g�sterilece�i alan (GridLayout i�inde)
    [SerializeField] GameObject bronzePackagePrefab;  // Bronze paketi prefab'�
    [SerializeField] GameObject silverPackagePrefab;  // Silver paketi prefab'�
    [SerializeField] GameObject goldPackagePrefab;    // Gold paketi prefab'�

    [SerializeField] GameObject cardScreen;
    [SerializeField] GameObject packageScreen;

    private string jsonFilePath;

    [SerializeField] CardManager cardManager; // CardManager referans�

    private void Awake()
    {
        myPackageButton.onClick.AddListener(() => { ShowUserPackages(); });
        jsonFilePath = Path.Combine(Application.persistentDataPath, "mypackages.json");  // Dosya yolunu belirle
    }

    void ShowUserPackages()
    {
        // �nceden eklenmi� paketleri temizle
        foreach (Transform child in packageParentObject.transform)
        {
            Destroy(child.gameObject);
        }

        // JSON dosyas�n� oku ve paketleri y�kle
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);

            // JSON verisini Dictionary �eklinde deserialize et
            var packageData = JsonUtility.FromJson<PackagesDictionary>(json);

            // E�er paket verisi varsa
            if (packageData != null)
            {
                // Paketleri kontrol et ve prefablar� ekle
                foreach (var package in packageData.packages)
                {
                    if (package.count > 0) // Paket say�s� 0'dan b�y�kse
                    {
                        GameObject packagePrefab = null;

                        // Paket ad�na g�re prefab se�
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

                        // E�er prefab bulunduysa, sadece 1 tane instantiation yap
                        if (packagePrefab != null)
                        {
                            GameObject instantiatedPackage = Instantiate(packagePrefab, packageParentObject.transform);

                            // PackagePrefab'daki "countText" TextMeshProUGUI bile�enini bul
                            TextMeshProUGUI countText = instantiatedPackage.GetComponentInChildren<TextMeshProUGUI>();

                            if (countText != null)
                            {
                                // countText'e count de�erini ata
                                countText.text = $"x{package.count.ToString()}";
                            }
                            else
                            {
                                Debug.LogWarning("Prefab i�erisinde 'countText' adl� TextMeshProUGUI bile�eni bulunamad�.");
                            }

                            // Paket i�indeki butona referans al
                            Button packageButton = instantiatedPackage.transform.Find("Button")?.GetComponent<Button>();

                            if (packageButton != null)
                            {
                                // Butona t�klama event'i ekle
                                packageButton.onClick.AddListener(() =>
                                {
                                    // Paket say�s�n� 1 eksilt
                                    if (package.count > 0)
                                    {
                                        package.count--;
                                        // TextMeshPro i�erisine yeni say�y� ata
                                        countText.text = $"x{package.count.ToString()}";
                                        Debug.Log($"{package.name} paketi say�s�: {package.count}");

                                        packageScreen.SetActive(false);
                                        cardScreen.SetActive(true);

                                        // Hangi paketin a��ld���n� CardManager'a g�nder
                                        cardManager.SetPackageType(package.name);

                                        // Say� s�f�r oldu�unda butonu devre d��� b�rak
                                        if (package.count == 0)
                                        {
                                            packageButton.interactable = false;
                                        }

                                        // JSON dosyas�n� g�ncelle ve kaydet
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
            Debug.LogWarning("Paket JSON dosyas� bulunamad�.");
        }
    }

    void SaveUpdatedPackageData(PackagesDictionary packageData)
    {
        // G�ncellenmi� paket verisini JSON format�nda serialize et
        string updatedJson = JsonUtility.ToJson(packageData, true);

        // JSON verisini dosyaya kaydet
        File.WriteAllText(jsonFilePath, updatedJson);

        Debug.Log("Paket verisi kaydedildi.");
    }

}

// Paket verisinin JSON format�na uygun olarak deserialize edilebilmesi i�in
[System.Serializable]
public class PackagesDictionary
{
    public List<PackageData> packages;
}
