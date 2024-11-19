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
    [SerializeField] private GameObject userCardPrefab;  // Kart prefab'�n� buraya al�yoruz
    [SerializeField] private Transform userCardsParent;  // Kartlar�n yerle�tirilece�i ana obje

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
            Debug.LogWarning("Futbolcu bilgileri JSON dosyas� bulunamad�.");
            return;
        }

        string json = File.ReadAllText(jsonFilePath);
        FootballerInfoList footballerInfoList = JsonUtility.FromJson<FootballerInfoList>(json);

        if (footballerInfoList != null && footballerInfoList.footballers.Count > 0)
        {
            // Kartlar� grid d�zeninde yerle�tir
            CreateGridWithEmptyParent(footballerInfoList.footballers);
        }
        else
        {
            Debug.LogWarning("JSON dosyas�nda futbolcu bilgisi bulunamad�.");
        }
    }

    private void CreateGridWithEmptyParent(List<FootballerInfo> footballers)
    {
        GameObject parent = new GameObject("GridParent");
        parent.transform.SetParent(userCardsParent); // Kartlar ana objenin alt�nda olacak

        // Kart prefab'�n�n yerel (local) konumunu ve rotas�n� al�n
        Vector3 prefabPosition = userCardPrefab.transform.localPosition;
        Quaternion prefabRotation = userCardPrefab.transform.localRotation;

        // Kartlar� serbest bir �ekilde yerle�tirmek i�in pozisyon hesapla
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;  // Sat�r hesaplama
            int column = i % columns;  // S�tun hesaplama

            // Kartlar�n pozisyonunu prefab konumuna g�re ayarlay�n
            Vector3 localPosition = new Vector3(column * xSpacing, 0, row * zSpacing);

            // Kartlar� yerle�tirirken prefab'�n yerel konumunu ve d�n���n� kullan�yoruz
            Vector3 position = prefabPosition + localPosition;

            // Kart� olu�tur ve konumland�r, d�n��� de kullan
            GameObject card = Instantiate(userCardPrefab, position, prefabRotation, parent.transform);

            ShowFootballerIn3D(card, footballers[i]); // Kart bilgilerini 3D'ye aktar
        }
    }

    private void ShowFootballerIn3D(GameObject card, FootballerInfo footballer)
    {
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
            priceText.text = footballer.price;

            // Oyuncunun kartlar�n�n g�rsellerini y�kleme
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
            Debug.LogError("Footballer GameObject'i bulunamad�.");
        }
    }
}