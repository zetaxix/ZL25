using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Globalization;

public class MyFootballers : MonoBehaviour
{
    [SerializeField] private GameObject footballerCardPrefab; // Kart prefab�
    [SerializeField] private Transform footballersParent; // Grid Layout Group i�eren parent objesi

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
        TextMeshProUGUI countText = card.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        RawImage cardBackground = card.GetComponent<RawImage>(); // Kart�n arka plan�

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        // UI ��elerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = FormatCurrency(footballer.price);
        countText.text = $"x{footballer.footballerCount}";

        // Paket t�r�ne g�re arka plan rengini de�i�tir
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
                cardBackground.color = Color.white; // Varsay�lan renk
                break;
        }

        // Oyuncunun kartlar�n�n g�rsellerini y�kleme
        playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
        countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
        teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);

        // T�klama olay�n� ekle
        Button cardButton = card.transform.Find("Button").GetComponent<Button>();
        cardButton.onClick.AddListener(() => ShowFootballerIn3D(footballer));
    }

    private void ShowFootballerIn3D(FootballerInfo footballer)
    {
        canvasScreen.SetActive(false);
        cardBackIcon.SetActive(true);

        // Yeni prefab olu�tur
        card = Instantiate(footballer3DPrefab, footballer3DPrefab.transform.position, footballer3DPrefab.transform.rotation);

        // Kart�n i�indeki UI ��elerini al
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Card'daki renderer bile�enine ula��n
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart �zerinde Renderer bile�eni bulunamad�.");
            return;
        }

        // De�i�tirilecek texture'� belirleyin
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

        // E�er bir texture se�ildiyse, t�m materyallerin ana texture'�n� g�ncelleyin
        if (selectedTexture != null)
        {
            foreach (Material mat in cardRenderer.materials)
            {
                mat.mainTexture = selectedTexture;
            }
        }

        // Footballer GameObject'inin i�indeki RawImage'lar� bulmak i�in tam yolu belirtin
        Transform footballerObject = card.transform.Find("Footballer");

        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // UI ��elerine futbolcu bilgilerini aktar
            nameText.text = footballer.name;
            ratingText.text = footballer.rating.ToString();
            priceText.text = FormatCurrency(footballer.price);

            // Oyuncunun kartlar�n�n g�rsellerini y�kleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamad�.");
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

        TextureCache.Instance.ClearCache();
    }

    public static string FormatCurrency(string priceString)
    {
        // Fiyat�n zaten formatl� olup olmad���n� kontrol et
        if (priceString.EndsWith("M�") || priceString.EndsWith("K�") || priceString.EndsWith("�"))
        {
            // E�er fiyat zaten formatl�ysa dokunma
            return priceString;
        }

        // Fiyat formatl� de�ilse i�leme devam et
        if (int.TryParse(priceString, out int price))
        {
            if (price >= 1000000)
            {
                return $"{(price / 1000000f).ToString("0.#", CultureInfo.InvariantCulture)}M�";
            }
            else if (price >= 1000)
            {
                return $"{(price / 1000f).ToString("0.#", CultureInfo.InvariantCulture)}K�";
            }
            else
            {
                return $"{price}�";
            }
        }
        else
        {
            Debug.LogError("Ge�ersiz fiyat format�: " + priceString);
            return "Hata!";
        }
    }

}
