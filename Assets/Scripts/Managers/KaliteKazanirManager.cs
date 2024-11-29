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
    [SerializeField] private GameObject userCardPrefab;  // Kart prefab'�n� buraya al�yoruz
    [SerializeField] private Transform userCardsParent;  // Kartlar�n yerle�tirilece�i ana obje

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

    #region Kart Koruma Ekran�nda Kullan�c�n�n destesini g�ster ve se�im yapt�r

    /// <summary>
    /// Kullan�c�n�n se�ti�i kartlar� JSON dosyas�ndan y�kler ve grid �eklinde g�sterir.
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
            Debug.LogWarning("chosenfootballers.json dosyas� bulunamad�.");
        }
    }

    /// <summary>
    /// Kullan�c�n�n kartlar�n� grid �eklinde g�sterir.
    /// </summary>
    private void CreateUserGrid(List<FootballerInfo> footballers)
    {
        // Grid i�in bir parent olu�tur
        GameObject parent = new GameObject("OpponentGridParent");
        parent.transform.SetParent(userCardsParent);

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = ForList3dopponentCardPrefab.transform.localPosition;
        Quaternion prefabRotation = ForList3dopponentCardPrefab.transform.localRotation;

        zSpacing = 0.08f;
        xSpacing = 0.12f;

        // Grid d�zeni olu�tur
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;    // Sat�r hesaplama
            int column = i % columns; // S�tun hesaplama

            // Kart�n pozisyonunu hesapla
            Vector3 localPosition = new Vector3(column * xSpacing, 0, row * zSpacing);
            Vector3 position = prefabPosition + localPosition;

            // Kart� olu�tur ve parent'a ekle
            GameObject card = Instantiate(ForList3dopponentCardPrefab, position, prefabRotation, parent.transform);

            // Kart se�me i�lemini y�netmek i�in t�klama olay�n� ba�la
            AddCardClickHandler(card);

            // Kart detaylar�n� doldur
            ShowFootballerIn3D(card, footballers[i]);
        }
    }

    /// <summary>
    /// Kart se�me i�levini y�netir.
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
                Debug.LogWarning("En fazla 3 kart se�ebilirsiniz.");
                return;
            }

            selectedCards.Add(card);
            renderer.material.color = selectedColor;
        }
    }

    /// <summary>
    /// Kullan�c�n�n se�ti�i kartlar� JSON format�nda kaydeder.
    /// </summary>
    public void SaveProtectedCards()
    {
        // Se�ilen kart bilgilerini tutacak liste
        List<FootballerInfo> selectedCardInfo = new List<FootballerInfo>();

        // Se�ilen kartlar�n kontrol edilmesi
        if (selectedCards == null || selectedCards.Count == 0)
        {
            Debug.LogError("Hi� kart se�ilmedi. L�tfen se�im yap�n.");
            return;
        }

        // Se�ilen kartlar �zerinden ge�erek bilgilerini toplama
        foreach (GameObject card in selectedCards)
        {
            if (card == null)
            {
                Debug.LogWarning("Bir kart referans� null. Atlan�yor.");
                continue;
            }

            // Kart�n �zerindeki bilgileri al
            TextMeshPro nameText = card.transform.Find("NameText")?.GetComponent<TextMeshPro>();
            TextMeshPro ratingText = card.transform.Find("RatingText")?.GetComponent<TextMeshPro>();
            TextMeshPro priceText = card.transform.Find("PriceText")?.GetComponent<TextMeshPro>();

            // G�rselleri ve di�er bilgileri al
            Transform footballerObject = card.transform.Find("Footballer");
            RawImage playerImage = footballerObject?.Find("FootballerImage")?.GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject?.Find("CountryFlag")?.GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject?.Find("TeamLogo")?.GetComponent<RawImage>();

            // Kart�n packageType'�n� belirleme
            string packageType = "Unknown"; // Varsay�lan de�er
            Renderer cardRenderer = card.GetComponent<Renderer>();
            if (cardRenderer != null)
            {
                // Materyalin ana texture'� �zerinden packageType belirleme
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
                Debug.LogError("Kart �zerinde Renderer bile�eni bulunamad�. Varsay�lan packageType kullan�lacak.");
            }

            // Kart bilgilerini olu�tur
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
                    footballerCount = 1, // Her bir kart i�in varsay�lan olarak 1
                    packageType = packageType
                };

                selectedCardInfo.Add(footballer);
                Debug.Log($"Se�ilen futbolcu eklendi: {footballer.name}");
            }
            else
            {
                Debug.LogError("Kartta eksik bilgi var. �sim, rating veya fiyat bilgisi al�namad�.");
            }
        }

        // Se�ilen kartlar listesinin JSON dosyas�na kaydedilmesi
        if (selectedCardInfo.Count > 0)
        {
            string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
            string filePath = Path.Combine(Application.persistentDataPath, "userProtectedCards.json");

            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Koruma kartlar� ba�ar�yla kaydedildi: {filePath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"JSON dosyas� kaydedilemedi: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Hi�bir kart bilgisi kaydedilmedi.");
        }

        // Se�ilen kartlar listesinin JSON dosyas�na kaydedilmesi
        if (selectedCardInfo.Count > 0)
        {
            string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
            string filePath = Path.Combine(Application.persistentDataPath, "userProtectedCards.json");

            try
            {
                File.WriteAllText(filePath, json);
                Debug.Log($"Koruma kartlar� ba�ar�yla kaydedildi: {filePath}");
            }
            catch (IOException ex)
            {
                Debug.LogError($"JSON dosyas� kaydedilemedi: {ex.Message}");
            }
        }
        else
        {
            Debug.LogError("Hi�bir kart bilgisi kaydedilmedi.");
        }
    }



    /// <summary>
    /// Kart t�klama i�lemi i�in collider ve handler ekler.
    /// </summary>
    private void AddCardClickHandler(GameObject card)
    {
        card.AddComponent<BoxCollider>();
        card.AddComponent<CardClickHandler>();
    }

    /// <summary>
    /// Bilgisayar�n kullan�c�n�n kartlar�ndan 3 tanesini rastgele �almas�n� sa�lar ve JSON dosyas�na kaydeder.
    /// </summary>
    public void SaveAIStoleCardsToJson()
    {
        // Kullan�c�n�n kartlar�n� y�kle
        if (File.Exists(jsonFilePath))
        {
            string json = File.ReadAllText(jsonFilePath);
            FootballerInfoListWrapper wrapper = JsonUtility.FromJson<FootballerInfoListWrapper>(json);

            if (wrapper != null && wrapper.footballers.Count > 0)
            {
                // Kullan�c�n�n 7 kartl�k destesinden rastgele 3 kart se�
                List<FootballerInfo> stolenCards = GetRandomFootballers(wrapper.footballers, MaxSelectableCards);

                // JSON sar�c� s�n�f�
                FootballerInfoListWrapper stolenWrapper = new FootballerInfoListWrapper { footballers = stolenCards };

                // JSON string'e d�n��t�r
                string stolenJson = JsonUtility.ToJson(stolenWrapper, true);

                // Dosya yolunu olu�tur
                string filePath = Path.Combine(Application.persistentDataPath, "AIStoleCards.json");

                try
                {
                    // JSON'u dosyaya yaz
                    File.WriteAllText(filePath, stolenJson);
                    Debug.Log($"Bilgisayar�n �ald��� kartlar kaydedildi: {filePath}");
                }
                catch (IOException ex)
                {
                    Debug.LogError($"JSON dosyas� kaydedilemedi: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("Kullan�c�n�n kartlar� bulunamad� veya liste bo�.");
            }
        }
        else
        {
            Debug.LogWarning("chosenfootballers.json dosyas� bulunamad�.");
        }
    }

    /// <summary>
    /// Belirtilen say�da futbolcuyu rastgele se�er.
    /// </summary>
    private List<FootballerInfo> GetRandomFootballers(List<FootballerInfo> sourceList, int count)
    {
        List<FootballerInfo> randomList = new List<FootballerInfo>();
        List<FootballerInfo> copyList = new List<FootballerInfo>(sourceList); // Orijinal listeyi bozmamak i�in kopya olu�tur

        for (int i = 0; i < count && copyList.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, copyList.Count);
            randomList.Add(copyList[randomIndex]);
            copyList.RemoveAt(randomIndex); // Se�ilen futbolcuyu listeden ��kar
        }

        return randomList;
    }

    public void HideUserGridParent()
    {
        GameObject userGridParent = userCardsParent.Find("OpponentGridParent").gameObject;
        userGridParent.SetActive(false);
    }

    #endregion

    #region Oyun ekran�nda kullan�c�n�n se�ti�i kartlar� g�ster

    public void LoadAndDisplayFootballers()
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

    #endregion
}