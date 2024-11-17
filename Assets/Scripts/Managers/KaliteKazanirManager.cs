using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class KaliteKazanirManager : MonoBehaviour
{
    [Header("Packages Textures")]
    [SerializeField] Texture2D BronzePackage;
    [SerializeField] Texture2D SilverPackage;
    [SerializeField] Texture2D GoldPackage;

    [SerializeField] TextMeshProUGUI opponentText;
    [SerializeField] TextMeshProUGUI usernameText;

    [Header("Footballer Card Setting")]
    [SerializeField] private GameObject footballerCardPrefab; // Kart prefabý
    [SerializeField] private Transform footballersParent; // Grid Layout Group içeren parent objesi

    // Json veri çekme ve veri kaydetmek için kullanýlan deðiþkenler
    private string jsonFilePath;

    private void Awake()
    {
        opponentText.text = PlayerPrefs.GetString("opponentUsername");
        usernameText.text = PlayerPrefs.GetString("username");
    }

    private void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "chosenfootballers.json");


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
        RawImage cardBackground = card.GetComponent<RawImage>(); // Kartýn arka planý

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        Button cardSelectButton = card.transform.Find("SelectButton").GetComponent<Button>();

        // UI öðelerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = footballer.price;

        // Kartýn seçimi için gerekli parametreler
        bool isSelected = false; // Bu kart seçili mi?
        Color selectedColor = Color.green; // Seçim rengi
        Color defaultColor = Color.white; // Varsayýlan renk

        // Paket türüne göre arka plan dokusunu ayarla
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
                cardBackground.color = defaultColor; // Varsayýlan renk
                break;
        }

        // Oyuncunun kartlarýnýn görsellerini yükleme
        playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
        countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
        teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);

        // Kart seçimini yönet
        cardSelectButton.onClick.AddListener(() =>
        {
            if (!isSelected)
            {
                Debug.Log($"{footballer.name} choosed!");
                isSelected = true;
            }
            else if (isSelected)
            {
                Debug.Log($"{footballer.name} unchoosed!");
                isSelected= false;
            }

        });
    }
}
