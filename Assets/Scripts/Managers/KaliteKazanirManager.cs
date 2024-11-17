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
    [SerializeField] private GameObject footballerCardPrefab; // Kart prefab�
    [SerializeField] private Transform footballersParent; // Grid Layout Group i�eren parent objesi

    // Json veri �ekme ve veri kaydetmek i�in kullan�lan de�i�kenler
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
            Debug.LogWarning("Futbolcu bilgileri JSON dosyas� bulunamad�.");
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
            Debug.LogWarning("JSON dosyas�nda futbolcu bilgisi bulunamad�.");
        }
    }

    private void CreateFootballerCard(FootballerInfo footballer)
    {
        GameObject card = Instantiate(footballerCardPrefab, footballersParent);

        // Kart�n i�indeki UI ��elerine ula�
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();
        RawImage cardBackground = card.GetComponent<RawImage>(); // Kart�n arka plan�

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        Button cardSelectButton = card.transform.Find("SelectButton").GetComponent<Button>();

        // UI ��elerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = footballer.price;

        // Kart�n se�imi i�in gerekli parametreler
        bool isSelected = false; // Bu kart se�ili mi?
        Color selectedColor = Color.green; // Se�im rengi
        Color defaultColor = Color.white; // Varsay�lan renk

        // Paket t�r�ne g�re arka plan dokusunu ayarla
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
                cardBackground.color = defaultColor; // Varsay�lan renk
                break;
        }

        // Oyuncunun kartlar�n�n g�rsellerini y�kleme
        playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
        countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
        teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);

        // Kart se�imini y�net
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
