using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class KaliteKazanirManager : MonoBehaviour
{
    [Header("Packages Textures")]
    [SerializeField] private Texture2D BronzePackage;
    [SerializeField] private Texture2D SilverPackage;
    [SerializeField] private Texture2D GoldPackage;

    [Header("User Cards")]
    [SerializeField] private GameObject userCardPrefab;  // Kart prefab'ýný buraya alýyoruz
    [SerializeField] private Transform userCardsParent;  // Kartlarýn yerleþtirileceði ana obje

    [Header("Grid Settings")]
    [SerializeField] float xSpacing;
    [SerializeField] float zSpacing;
    [SerializeField] int columns;

    [Header("Text Settings")]
    [SerializeField] TextMeshPro usernameText;
    [SerializeField] TextMeshPro opponentUsernameText;

    private string jsonFilePath;

    private void Awake()
    {
        usernameText.text = PlayerPrefs.GetString("username");
        opponentUsernameText.text = PlayerPrefs.GetString("opponentUsername");
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
            // Kartlarý grid düzeninde yerleþtir
            CreateGridWithEmptyParent(footballerInfoList.footballers);
        }
        else
        {
            Debug.LogWarning("JSON dosyasýnda futbolcu bilgisi bulunamadý.");
        }
    }

    private void CreateGridWithEmptyParent(List<FootballerInfo> footballers)
    {
        GameObject parent = new GameObject("GridParent");
        parent.transform.SetParent(userCardsParent); // Kartlar ana objenin altýnda olacak

        // Kart prefab'ýnýn yerel (local) konumunu ve rotasýný alýn
        Vector3 prefabPosition = userCardPrefab.transform.localPosition;
        Quaternion prefabRotation = userCardPrefab.transform.localRotation;

        // Kartlarý serbest bir þekilde yerleþtirmek için pozisyon hesapla
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;  // Satýr hesaplama
            int column = i % columns;  // Sütun hesaplama

            // Kartlarýn pozisyonunu prefab konumuna göre ayarlayýn
            Vector3 localPosition = new Vector3(column * xSpacing, 0, row * zSpacing);

            // Kartlarý yerleþtirirken prefab'ýn yerel konumunu ve dönüþünü kullanýyoruz
            Vector3 position = prefabPosition + localPosition;

            // Kartý oluþtur ve konumlandýr, dönüþü de kullan
            GameObject card = Instantiate(userCardPrefab, position, prefabRotation, parent.transform);

            ShowFootballerIn3D(card, footballers[i]); // Kart bilgilerini 3D'ye aktar
        }
    }

    private void ShowFootballerIn3D(GameObject card, FootballerInfo footballer)
    {
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

            // Oyuncunun kartlarýnýn görsellerini yükleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);

            Transform canvas = card.transform.Find("Canvas");

            if (canvas != null)
            {
                Button cardButton = canvas.Find("Button").GetComponent<Button>();

                cardButton.onClick.AddListener(() =>
                {
                    Debug.Log(footballer.name);
                });
            }
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }
    }
}