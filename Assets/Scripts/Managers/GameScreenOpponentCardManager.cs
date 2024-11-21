using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.IO;
using System.Xml;

public class GameScreenOpponentCardManager : MonoBehaviour
{
    public static GameScreenOpponentCardManager Instance;

    [Header("Opponent Card Prefab")]
    [SerializeField] private GameObject opponentCardPrefab; // Rakip kart prefab'�
    [SerializeField] private GameObject ForList3dopponentCardPrefab; // Rakip kart prefab'�
    public GameObject opponentCard;
    public Transform opponentCardsParent;

    [Header("Card Textures")]
    [SerializeField] private Texture2D BronzeCardTexture;
    [SerializeField] private Texture2D SilverCardTexture;
    [SerializeField] private Texture2D GoldCardTexture;

    [Header("Opponent Footballer List")]
    [SerializeField] private List<FootballerInfo> opponentFootballerList = new List<FootballerInfo>();

    public FootballerInfo randomFootballer;

    [Header("Grid Settings")]
    [SerializeField] float xSpacing;
    [SerializeField] float zSpacing;
    [SerializeField] int columns;

    [Header("Last List Selection Settings")]
    [SerializeField] private Color selectedColor = Color.green; // Se�ili kart rengi
    private List<GameObject> selectedCards = new List<GameObject>(); // Se�ili kartlar�n listesi
    private const int MaxSelectableCards = 3; // Maksimum se�ilebilir kart say�s�

    void Awake()
    {
        // instance bo�sa bu scripti ata
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // E�er ba�ka bir Instance varsa onu yok et
        }
    }

    #region 3DOpponentCard listeli bir �ekilde olu�turup GridParent ile g�sterme

    public void StartShowOpponentCards()
    {
        // T�m futbolcular� gridden g�ster
        if (opponentFootballerList.Count > 0)
        {
            ShowRandomOpponentGrid();
        }
        else
        {
            Debug.LogWarning("Rakip i�in g�sterilecek futbolcu bilgisi bulunamad�.");
        }
    }

    /// <summary>
    /// Rastgele 7 futbolcuyu se�ip grid �eklinde g�sterir.
    /// </summary>
    private void ShowRandomOpponentGrid()
    {
        // Rastgele 7 futbolcu se�
        List<FootballerInfo> randomFootballers = GetRandomFootballers(opponentFootballerList, 7);

        // Se�ilen futbolcular� grid �eklinde g�ster
        CreateOpponentGrid(randomFootballers);
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

    /// <summary>
    /// Belirtilen futbolcu listesini grid �eklinde g�sterir.
    /// </summary>
    private void CreateOpponentGrid(List<FootballerInfo> footballers)
    {
        // Grid i�in bir parent olu�tur
        GameObject parent = new GameObject("OpponentGridParent");
        parent.transform.SetParent(opponentCardsParent);

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = ForList3dopponentCardPrefab.transform.localPosition;
        Quaternion prefabRotation = ForList3dopponentCardPrefab.transform.localRotation;

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
            ShowOpponentCardDetails(card, footballers[i]);
        }
    }

    private void AddCardClickHandler(GameObject card)
    {
        card.AddComponent<BoxCollider>(); // T�klama alg�lamas� i�in collider ekle
        card.AddComponent<CardClickHandler>(); // T�klama i�lemini y�netecek script ekle
    }

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
    /// Bilgisayar�n korunacak 3 kart�n� rastgele se�ip JSON dosyas�na kaydeder.
    /// </summary>
    public void SaveAIProtectedCardsToJson()
    {
        // 7 karttan 3 tane rastgele se�
        List<FootballerInfo> protectedCards = GetRandomFootballers(opponentFootballerList, 3);

        // JSON sar�c� s�n�f�
        FootballerInfoListWrapper wrapper = new FootballerInfoListWrapper { footballers = protectedCards };

        // JSON string'e d�n��t�r
        string json = JsonUtility.ToJson(wrapper, true);

        // Dosya yolunu olu�tur
        string filePath = Path.Combine(Application.persistentDataPath, "aiProtectedCard.json");

        try
        {
            // JSON'u dosyaya yaz
            File.WriteAllText(filePath, json);
            Debug.Log($"Bilgisayar�n korudu�u kartlar kaydedildi: {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"JSON dosyas� kaydedilemedi: {ex.Message}");
        }
    }

    public void HideOpponentGridParent()
    {
        GameObject opponentCardParentobj = opponentCardsParent.Find("OpponentGridParent").gameObject;
        opponentCardParentobj.SetActive(false);
    }

    public void SaveUserSelectedCardsToJson()
    {
        List<FootballerInfo> selectedCardInfo = new List<FootballerInfo>();

        if (selectedCards == null || selectedCards.Count == 0)
        {
            Debug.LogError("Hi� kart se�ilmedi. Se�im i�lemini kontrol edin.");
            return;
        }

        // Se�ilen kartlar�n isimlerini al
        List<string> selectedCardNames = new List<string>();
        foreach (GameObject card in selectedCards)
        {
            if (card == null)
            {
                Debug.LogWarning("Bir kart referans� null. Atlan�yor.");
                continue;
            }

            TextMeshPro nameText = card.transform.Find("NameText")?.GetComponent<TextMeshPro>();
            if (nameText != null)
            {
                string cardName = nameText.text.Trim();
                if (!string.IsNullOrEmpty(cardName))
                {
                    selectedCardNames.Add(cardName);
                    Debug.Log($"Se�ilen kart ismi al�nd�: {cardName}");
                }
                else
                {
                    Debug.LogWarning("Kart�n ismi bo�.");
                }
            }
            else
            {
                Debug.LogError("Kart �zerinde NameText bile�eni bulunamad�.");
            }
        }

        if (selectedCardNames.Count == 0)
        {
            Debug.LogError("Se�ilen kartlar aras�nda isim bilgisi al�namad�.");
            return;
        }

        // �simlere g�re opponentFootballerList'ten futbolcular� bul
        foreach (string cardName in selectedCardNames)
        {
            FootballerInfo matchedFootballer = opponentFootballerList.Find(f => f.name == cardName);
            if (matchedFootballer != null)
            {
                selectedCardInfo.Add(matchedFootballer);
                Debug.Log($"Se�ilen futbolcu bilgisi eklendi: {matchedFootballer.name}");
            }
            else
            {
                Debug.LogWarning($"Se�ilen kart ismi ({cardName}) opponentFootballerList'te bulunamad�.");
            }
        }

        if (selectedCardInfo.Count == 0)
        {
            Debug.LogError("Se�ili kartlar aras�nda kaydedilecek ge�erli futbolcu bulunamad�.");
            return;
        }

        // JSON dosyas�na kaydet
        string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
        string filePath = Path.Combine(Application.persistentDataPath, "stoleCards.json");

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Se�ilen kartlar kaydedildi: {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"JSON dosyas� kaydedilemedi: {ex.Message}");
        }
    }

    /// <summary>
    /// Kart detaylar�n� doldurur.
    /// </summary>
    private void ShowOpponentCardDetails(GameObject card, FootballerInfo footballer)
    {
        // UI ��elerine eri�im
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Kart�n Renderer'�na eri�im
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart �zerinde Renderer bile�eni bulunamad�.");
            return;
        }

        // Kart�n texturesini belirleme
        Texture2D selectedTexture = null;
        switch (footballer.packageType)
        {
            case "Bronze":
                selectedTexture = BronzeCardTexture;
                break;
            case "Silver":
                selectedTexture = SilverCardTexture;
                break;
            case "Gold":
                selectedTexture = GoldCardTexture;
                break;
            default:
                Debug.LogError("Bilinmeyen kart t�r�!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // UI bilgilerini g�ncelleme
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;

        // Fiyat� bi�imlendirerek g�stermek
        int price = 0;
        if (int.TryParse(footballer.price, out price))
        {
            priceText.text = FormatPrice(price);
        }
        else
        {
            priceText.text = "Invalid Price";
        }

        // G�rselleri y�kleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // G�rselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamad�.");
        }
    }

    /// <summary>
    /// Kart�n texture'�n� ayarlar.
    /// </summary>
    private void SetCardTexture(Renderer renderer, Texture2D texture)
    {
        foreach (Material mat in renderer.materials)
        {
            mat.mainTexture = texture;
        }
    }
    #endregion

    #region Rastgele 1 Futbolcu Se� ve User'a Kar�� At
    public void GenerateandShowOpponentCard()
    {
        // Rastgele bir futbolcu se� ve kart�n� g�ster
        if (opponentFootballerList.Count > 0)
        {
            randomFootballer = SelectRandomFootballer();
            ShowOpponentCard(randomFootballer);
        }
        else
        {
            Debug.LogError("Rakip futbolcu listesi bo�!");
        }
    }

    // Rastgele bir futbolcu se�me
    private FootballerInfo SelectRandomFootballer()
    {
        int randomIndex = Random.Range(0, opponentFootballerList.Count);
        return opponentFootballerList[randomIndex];
    }

    // Se�ilen futbolcunun kart�n� olu�turma ve sahneye yerle�tirme
    private void ShowOpponentCard(FootballerInfo footballer)
    {
        // Kart� olu�tur ve pozisyonland�r
        opponentCard = Instantiate(opponentCardPrefab, opponentCardPrefab.transform.position, opponentCardPrefab.transform.rotation);

        // Kart bilgilerini doldur
        PopulateCardDetails(opponentCard, footballer);
    }

    // Kart detaylar�n� doldurma
    private void PopulateCardDetails(GameObject card, FootballerInfo footballer)
    {
        // UI ��elerine eri�im
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Kart�n Renderer'�na eri�im
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart �zerinde Renderer bile�eni bulunamad�.");
            return;
        }

        // Kart�n texturesini belirleme
        Texture2D selectedTexture = null;
        switch (footballer.packageType)
        {
            case "Bronze":
                selectedTexture = BronzeCardTexture;
                break;
            case "Silver":
                selectedTexture = SilverCardTexture;
                break;
            case "Gold":
                selectedTexture = GoldCardTexture;
                break;
            default:
                Debug.LogError("Bilinmeyen kart t�r�!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // UI bilgilerini g�ncelleme
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;

        // Fiyat� bi�imlendirerek g�stermek
        int price = 0;
        if (int.TryParse(footballer.price, out price))
        {
            priceText.text = FormatPrice(price);
        }
        else
        {
            priceText.text = "Invalid Price";
        }

        // G�rselleri y�kleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // G�rselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamad�.");
        }
    }

    private string FormatPrice(int price)
    {
        if (price >= 1000000000)
        {
            return (price / 1000000000f).ToString("0.0") + "B�"; // 1B = 1 Billion = 1 Milyar
        }
        else if (price >= 1000000)
        {
            return (price / 1000000f).ToString("0.0") + "M�"; // 1M = 1 Million = 1 Milyon
        }
        else if (price >= 1000)
        {
            return (price / 1000f).ToString("0.0") + "K�"; // 1K = 1 Thousand = 1 Bin
        }
        else
        {
            return price.ToString() + "�"; // K���k fiyatlar i�in direkt g�sterim
        }
    }

    #endregion

}

// JSON sar�c� s�n�f�
[System.Serializable]
public class FootballerInfoListWrapper
{
    public List<FootballerInfo> footballers;
}

