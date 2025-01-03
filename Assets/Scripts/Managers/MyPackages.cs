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

    [SerializeField] GameObject cardContunieBtn;

    [Header("Stads Photos")]
    [SerializeField] RawImage stadRawImage;

    [SerializeField] Texture bronzeStadTexture;
    [SerializeField] Texture silverStadTexture;
    [SerializeField] Texture goldStadTexture;

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
                                packageButton.onClick.AddListener(() =>
                                {
                                    // Paket say�s�n� azalt
                                    if (package.count > 0)
                                    {
                                        package.count--;
                                        countText.text = $"x{package.count.ToString()}";
                                        Debug.Log($"{package.name} paketi say�s�: {package.count}");

                                        // Paket ekranlar�n� y�net
                                        packageScreen.SetActive(false);
                                        cardScreen.SetActive(true);

                                        // G�rseli de�i�tir
                                        UpdateStadImage(package.name);

                                        // CardManager'a paketin t�r�n� bildir
                                        cardManager.SetPackageType(package.name);

                                        // JSON dosyas�n� g�ncelle
                                        SaveUpdatedPackageData(packageData);

                                        // Devam butonunu gecikmeli olarak g�ster
                                        StartCoroutine(ContunieButtonDelay());

                                        // Say� s�f�rsa prefab'� kald�r
                                        if (package.count == 0)
                                        {
                                            packageButton.interactable = false;
                                            Destroy(instantiatedPackage);
                                        }
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

    void UpdateStadImage(string packageType)
    {
        switch (packageType)
        {
            case "Bronze":
                stadRawImage.texture = bronzeStadTexture;
                stadRawImage.gameObject.SetActive(true);

                break;
            case "Silver":
                stadRawImage.texture = silverStadTexture;
                stadRawImage.gameObject.SetActive(true);

                break;
            case "Gold":
                stadRawImage.texture = goldStadTexture;
                stadRawImage.gameObject.SetActive(true);

                break;
            default:
                Debug.LogWarning("Bilinmeyen paket t�r�!");
                break;
        }

        Debug.Log($"{packageType} paketinin g�rseli ayarland�.");
    }


    IEnumerator ContunieButtonDelay()
    {
        yield return new WaitForSeconds(4.5f);
        cardContunieBtn.SetActive(true);
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
