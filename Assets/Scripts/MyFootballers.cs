using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MyFootballers : MonoBehaviour
{
    [SerializeField] private GameObject footballerCardPrefab; // Kart prefabý
    [SerializeField] private Transform footballersParent; // Grid Layout Group içeren parent objesi

    [SerializeField] private GameObject footballer3DPrefab; // 3D Prefab

    private string jsonFilePath;

    [Header("Packages Textures")]
    [SerializeField] Texture2D BronzePackage;
    [SerializeField] Texture2D SilverPackage;
    [SerializeField] Texture2D GoldPackage;

    [SerializeField] GameObject canvasScreen;
    [SerializeField] GameObject cardBackIcon;

    GameObject card;

    private void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "myfootballers.json");
        LoadAndDisplayFootballers();
    }

    private void LoadAndDisplayFootballers()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogWarning("Futbolcu bilgileri JSON dosyasý bulunamadý.");
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        FootballerInfoList footballerInfoList = JsonUtility.FromJson<FootballerInfoList>(json);

        if (footballerInfoList != null && footballerInfoList.footballers.Count > 0)
        {
            foreach (FootballerInfo footballer in footballerInfoList.footballers)
            {
                CreateFootballerCard(footballer);
            }
        }
        else
        {
            Debug.LogWarning("JSON dosyasýnda futbolcu bilgisi bulunamadý.");
        }
    }
    private void CreateFootballerCard(FootballerInfo footballer)
    {
        GameObject card = Instantiate(footballerCardPrefab, footballersParent);

        // Kartýn içindeki UI öðelerine ulaþ
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI countText = card.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        RawImage cardBackground = card.GetComponent<RawImage>(); // Kartýn arka planý

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        // UI öðelerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = footballer.price;
        countText.text = $"x{footballer.footballerCount}";

        // Paket türüne göre arka plan rengini deðiþtir
        switch (footballer.packageType)
        {
            case "Bronze":
                cardBackground.texture = BronzePackage;
                break;
            case "Silver":
                cardBackground.texture = SilverPackage;
                break;
            case "Gold":
                cardBackground.texture = GoldPackage;
                break;
            default:
                cardBackground.color = Color.white; // Varsayýlan renk
                break;
        }

        // Texture'larý yükle ve UI'ya ata
        playerImage.texture = LoadTextureByPath("MyRepository/FootballerPhotos", footballer.playerImageName);
        countryFlagImage.texture = LoadTextureByPath("MyRepository/CountryPhotos", footballer.countryFlagImageName);
        teamLogoImage.texture = LoadTextureByPath("MyRepository/TeamPhotos", footballer.teamLogoImageName);

        // Týklama olayýný ekle
        Button cardButton = card.transform.Find("Button").GetComponent<Button>();
        cardButton.onClick.AddListener(() => ShowFootballerIn3D(footballer));
    }

    private Texture2D LoadTextureByPath(string folderPath, string fileName)
    {
        string fullPath = Path.Combine(Application.dataPath, folderPath, fileName + ".png");

        if (File.Exists(fullPath))
        {
            byte[] fileData = File.ReadAllBytes(fullPath);
            Texture2D texture = new Texture2D(2, 2); // Boyutlar otomatik olarak ayarlanacak
            if (texture.LoadImage(fileData))
            {
                return texture;
            }
            else
            {
                Debug.LogError($"Texture yüklenirken hata oluþtu: {fullPath}");
                return null;
            }
        }
        else
        {
            Debug.LogError($"Dosya bulunamadý: {fullPath}");
            return null;
        }
    }

    private void ShowFootballerIn3D(FootballerInfo footballer)
    {
        canvasScreen.SetActive(false);
        cardBackIcon.SetActive(true);

        // Yeni prefab oluþtur
        card = Instantiate(footballer3DPrefab, footballer3DPrefab.transform.position, footballer3DPrefab.transform.rotation);

        // Kartýn içindeki UI öðelerini al
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Card'daki renderer bileþenine ulaþýn
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileþeni bulunamadý.");
            return;
        }

        // Deðiþtirilecek texture'ý belirleyin
        Texture2D selectedTexture = null;
        if (footballer.packageType == "Bronze")
        {
            selectedTexture = BronzePackage;
        }
        else if (footballer.packageType == "Silver")
        {
            selectedTexture = SilverPackage;
        }
        else if (footballer.packageType == "Gold")
        {
            selectedTexture = GoldPackage;
        }

        // Eðer bir texture seçildiyse, tüm materyallerin ana texture'ýný güncelleyin
        if (selectedTexture != null)
        {
            foreach (Material mat in cardRenderer.materials)
            {
                mat.mainTexture = selectedTexture;
            }
        }

        // Footballer GameObject'inin içindeki RawImage'larý bulmak için tam yolu belirtin
        Transform footballerObject = card.transform.Find("Footballer");

        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // UI öðelerine futbolcu bilgilerini aktar
            nameText.text = footballer.name;
            ratingText.text = footballer.rating.ToString();
            priceText.text = footballer.price;

            // Texture'larý yükle ve UI'ya ata
            playerImage.texture = LoadTextureByPath("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = LoadTextureByPath("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = LoadTextureByPath("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }
    }

    public void CloseThe3DCard()
    {
        if (card != null)
        {
            Destroy(card);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene(1);
    }
}
