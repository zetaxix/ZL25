using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI moneyText;
    private string jsonFilePath;

    [Header("Buttons")]
    [SerializeField] Button BronzeButton;
    [SerializeField] Button SilverButton;
    [SerializeField] Button GoldButton;

    private void Awake()
    {
        BronzeButton.onClick.AddListener(() => BuyPackage("Bronze"));
        SilverButton.onClick.AddListener(() => BuyPackage("Silver"));
        GoldButton.onClick.AddListener(() => BuyPackage("Gold"));

        PlayerPrefs.SetInt("money", 100000);
    }

    private void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "mypackages.json");
        InitializePackagesJson();
    }

    private void Update()
    {
        UpdateMoneyUI();
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(1);
    }

    private void UpdateMoneyUI()
    {
        int currentMoney = PlayerPrefs.GetInt("money", 1000);
        moneyText.text = $"Money: €{currentMoney.ToString("N0")}";
    }

    private void InitializePackagesJson()
    {
        if (!File.Exists(jsonFilePath))
        {
            PackageDataContainer initialData = new PackageDataContainer
            {
                packages = new List<PackageData>
                {
                    new PackageData { name = "Bronze", count = 0 },
                    new PackageData { name = "Silver", count = 0 },
                    new PackageData { name = "Gold", count = 0 }
                }
            };

            string json = JsonUtility.ToJson(initialData, true);
            File.WriteAllText(jsonFilePath, json);
        }
    }

    public void BuyPackage(string packageType)
    {
        int currentMoney = PlayerPrefs.GetInt("money", 1000);
        int packageCost = GetPackageCost(packageType);

        if (currentMoney >= packageCost)
        {
            currentMoney -= packageCost;
            PlayerPrefs.SetInt("money", currentMoney);
            UpdateMoneyUI();
            Debug.Log($"{packageType} paketi satýn alýndý. Kalan para: {currentMoney}");

            // JSON dosyasýna satýn alma iþlemini kaydet
            UpdatePackageJson(packageType);
        }
        else
        {
            Debug.Log("Yetersiz bakiye!");
        }
    }

    private int GetPackageCost(string packageType)
    {
        return packageType switch
        {
            "Bronze" => 20,
            "Silver" => 40,
            "Gold" => 100,
            _ => 0
        };
    }

    private void UpdatePackageJson(string packageType)
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            PackageDataContainer packageDataContainer = JsonUtility.FromJson<PackageDataContainer>(json);

            foreach (var package in packageDataContainer.packages)
            {
                if (package.name == packageType)
                {
                    package.count += 1;
                    break;
                }
            }

            json = JsonUtility.ToJson(packageDataContainer, true);
            File.WriteAllText(jsonFilePath, json);
        }
    }
}

[System.Serializable]
public class PackageData
{
    public string name;
    public int count;
}

[System.Serializable]
public class PackageDataContainer
{
    public List<PackageData> packages;
}