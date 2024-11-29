using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class KaliteKazanirManager : MonoBehaviour
{
    public static KaliteKazanirManager Instance;

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
    [SerializeField] TextMeshProUGUI usernameText;
    [SerializeField] TextMeshProUGUI opponentUsernameText;

    [Header("Selection Settings")]
    [SerializeField] private GameObject ForList3dopponentCardPrefab;
    [SerializeField] private Color selectedColor = Color.green;
    private List<GameObject> selectedCards = new List<GameObject>();
    private const int MaxSelectableCards = 3;

    private string jsonFilePath;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        } else if (Instance != null)
        {
            Destroy(gameObject);
        }

        usernameText.text = PlayerPrefs.GetString("username");
        opponentUsernameText.text = PlayerPrefs.GetString("opponentUsername");
    }

    private void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "chosenfootballers.json");
        LoadAndDisplayFootballers();
    }

    #region Kart Koruma Ekranýnda Kullanýcýnýn destesini göster ve seçim yaptýr

    /// <summary>
    /// Kullanýcýnýn seçtiði kartlarý JSON dosyasýndan yükler ve grid þeklinde gösterir.
    /// </summary>
    public void LoadAndDisplayFootballersForProtectedScreen()
    {
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            FootballerInfoListWrapper wrapper = JsonUtility.FromJson<FootballerInfoListWrapper>(json);

            if (wrapper != null && wrapper.footballers.Count > 0)
            {
                CreateUserGrid(wrapper.footballers);
            }
        }
        else
        {
            Debug.LogWarning("chosenfootballers.json dosyasý bulunamadý.");
        }
    }

    /// <summary>
    /// Kullanýcýnýn kartlarýný grid þeklinde gösterir.
    /// </summary>
    private void CreateUserGrid(List<FootballerInfo> footballers)
    {
        // Grid için bir parent oluþtur
        GameObject parent = new GameObject("OpponentGridParent");
        parent.transform.SetParent(userCardsParent);

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = ForList3dopponentCardPrefab.transform.localPosition;
        Quaternion prefabRotation = ForList3dopponentCardPrefab.transform.localRotation;

        zSpacing = 0.08f;
        xSpacing = 0.12f;

        // Grid düzeni oluþtur
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;    // Satýr hesaplama
            int column = i % columns; // Sütun hesaplama

            // Kartýn pozisyonunu hesapla
            Vector3 localPosition = new Vector3(column * xSpacing, 0, row * zSpacing);
            Vector3 position = prefabPosition + localPosition;

            // Kartý oluþtur ve parent'a ekle
            GameObject card = Instantiate(ForList3dopponentCardPrefab, position, prefabRotation, parent.transform);

            // Kart seçme iþlemini yönetmek için týklama olayýný baðla
            AddCardClickHandler(card);

            // Kart detaylarýný doldur
            ShowFootballerIn3D(card, footballers[i]);
        }
    }

    /// <summary>
    /// Kart seçme iþlevini yönetir.
    /// </summary>
    public void ToggleCardSelection(GameObject card)
    {
        Renderer renderer = card.GetComponent<Renderer>();

        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            renderer.material.color = Color.white;
        }
        else
        {
            if (selectedCards.Count >= MaxSelectableCards)
            {
                Debug.LogWarning("En fazla 3 kart seçebilirsiniz.");
                return;
            }

            selectedCards.Add(card);
            renderer.material.color = selectedColor;
        }
    }

    /// <summary>
    /// Kullanýcýnýn seçtiði kartlarý JSON formatýnda kaydeder.
    /// </summary>
    public void SaveProtectedCards()
    {
        // Seçilen kart bilgilerini tutacak liste
        List<FootballerInfo> selectedCardInfo = new List<FootballerInfo>();

        // Seçilen kartlarýn kontrol edilmesi
        if (selectedCards == null || selectedCards.Count == 0)
        {
            Debug.LogError("Hiç kart seçilmedi. Lütfen seçim yapýn.");
            return;
        }

        // Seçilen kartlar üzerinden geçerek bilgilerini toplama
        foreach (GameObject card in selectedCards)
        {
            if (card == null)
            {
                Debug.LogWarning("Bir kart referansý null. Atlanýyor.");
                continue;
            }

            // Kartýn üzerindeki bilgileri al
            TextMeshPro nameText = card.transform.Find("NameText")?.GetComponent<TextMeshPro>();
            TextMeshPro ratingText = card.transform.Find("RatingText")?.GetComponent<TextMeshPro>();
            TextMeshPro priceText = card.transform.Find("PriceText")?.GetComponent<TextMeshPro>();

            // Görselleri ve diðer bilgileri al
            Transform footballerObject = card.transform.Find("Footballer");
            RawImage playerImage = footballerObject?.Find("FootballerImage")?.GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject?.Find("CountryFlag")?.GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject?.Find("TeamLogo")?.GetComponent<RawImage>();

            // Kartýn packageType'ýný belirleme
            string packageType = "Unknown"; // Varsayýlan deðer
            Renderer cardRenderer = card.GetComponent<Renderer>();
            if (cardRenderer != null)
            {
                // Materyalin ana texture'ý üzerinden packageType belirleme
                Texture2D mainTexture = cardRenderer.materials[0].mainTexture as Texture2D;
                if (mainTexture == BronzePackage)
                {
                    packageType = "Bronze";
                }
                else if (mainTexture == SilverPackage)
                {
                    packageType = "Silver";
                }
                else if (mainTexture == GoldPackage)
                {
                    packageType = "Gold";
                }
            }
            else
            {
                Debug.LogError("Kart üzerinde Renderer bileþeni bulunamadý. Varsayýlan packageType kullanýlacak.");
            }

            // Kart bilgilerini oluþtur
            if (nameText != null && ratingText != null && priceText != null)
            {
                FootballerInfo footballer = new FootballerInfo
                {
                    name = nameText.text.Trim(),
                    rating = ratingText.text.Trim(),
                    price = priceText.text.Trim(),
                    playerImageName = playerImage != null ? playerImage.texture.name : "",
                    countryFlagImageName = countryFlagImage != null ? countryFlagImage.texture.name : "",
                    teamLogoImageName = teamLogoImage != null ? teamLogoImage.texture.name : "",
                    footballerCount = 1, // Her bir kart için varsayýlan olarak 1
                    packageType = packageType
                };

                selectedCardInfo.Add(footballer);
                Debug.Log($"Seçilen futbolcu eklendi: {footballer.name}");
            }
            else
            {
                Debug.LogError("Kartta eksik bilgi var. Ýsim, rating veya fiyat bilgisi alýnamadý.");
            }
        }

        // Seçilen kartlar listesinin JSON dosyasýna kaydedilmesi
        if (selectedCardInfo.Count > 0)
        {
            string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
            string filePath = Path.Combine(Application.persistentDataPath, "userProtectedCards.json");

            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Koruma kartlarý baþarýyla kaydedildi: {filePath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"JSON dosyasý kaydedilemedi: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Hiçbir kart bilgisi kaydedilmedi.");
        }

        // Seçilen kartlar listesinin JSON dosyasýna kaydedilmesi
        if (selectedCardInfo.Count > 0)
        {
            string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
            string filePath = Path.Combine(Application.persistentDataPath, "userProtectedCards.json");

            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Koruma kartlarý baþarýyla kaydedildi: {filePath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"JSON dosyasý kaydedilemedi: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Hiçbir kart bilgisi kaydedilmedi.");
        }
    }



    /// <summary>
    /// Kart týklama iþlemi için collider ve handler ekler.
    /// </summary>
    private void AddCardClickHandler(GameObject card)
    {
        card.AddComponent<BoxCollider>();
        card.AddComponent<CardClickHandler>();
    }

    /// <summary>
    /// Bilgisayarýn kullanýcýnýn kartlarýndan 3 tanesini rastgele çalmasýný saðlar ve JSON dosyasýna kaydeder.
    /// </summary>
    public void SaveAIStoleCardsToJson()
    {
        // Kullanýcýnýn kartlarýný yükle
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            FootballerInfoListWrapper wrapper = JsonUtility.FromJson<FootballerInfoListWrapper>(json);

            if (wrapper != null && wrapper.footballers.Count > 0)
            {
                // Kullanýcýnýn 7 kartlýk destesinden rastgele 3 kart seç
                List<FootballerInfo> stolenCards = GetRandomFootballers(wrapper.footballers, MaxSelectableCards);

                // JSON sarýcý sýnýfý
                FootballerInfoListWrapper stolenWrapper = new FootballerInfoListWrapper { footballers = stolenCards };

                // JSON string'e dönüþtür
                string stolenJson = JsonUtility.ToJson(stolenWrapper, true);

                // Dosya yolunu oluþtur
                string filePath = Path.Combine(Application.persistentDataPath, "AIStoleCards.json");

                try
                {
                    // JSON'u dosyaya yaz
                    File.WriteAllText(filePath, stolenJson);
                    Debug.Log($"Bilgisayarýn çaldýðý kartlar kaydedildi: {filePath}");
                }
                catch (IOException ex)
                {
                    Debug.LogError($"JSON dosyasý kaydedilemedi: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Kullanýcýnýn kartlarý bulunamadý veya liste boþ.");
            }
        }
        else
        {
            Debug.LogWarning("chosenfootballers.json dosyasý bulunamadý.");
        }
    }

    /// <summary>
    /// Belirtilen sayýda futbolcuyu rastgele seçer.
    /// </summary>
    private List<FootballerInfo> GetRandomFootballers(List<FootballerInfo> sourceList, int count)
    {
        List<FootballerInfo> randomList = new List<FootballerInfo>();
        List<FootballerInfo> copyList = new List<FootballerInfo>(sourceList); // Orijinal listeyi bozmamak için kopya oluþtur

        for (int i = 0; i < count && copyList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, copyList.Count);
            randomList.Add(copyList[randomIndex]);
            copyList.RemoveAt(randomIndex); // Seçilen futbolcuyu listeden çýkar
        }

        return randomList;
    }

    public void HideUserGridParent()
    {
        GameObject userGridParent = userCardsParent.Find("OpponentGridParent").gameObject;
        userGridParent.SetActive(false);
    }

    #endregion

    #region Oyun ekranýnda kullanýcýnýn seçtiði kartlarý göster

    public void LoadAndDisplayFootballers()
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

    #endregion
}