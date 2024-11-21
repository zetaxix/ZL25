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
    [SerializeField] private GameObject opponentCardPrefab; // Rakip kart prefab'ý
    [SerializeField] private GameObject ForList3dopponentCardPrefab; // Rakip kart prefab'ý
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
    [SerializeField] private Color selectedColor = Color.green; // Seçili kart rengi
    private List<GameObject> selectedCards = new List<GameObject>(); // Seçili kartlarýn listesi
    private const int MaxSelectableCards = 3; // Maksimum seçilebilir kart sayýsý

    void Awake()
    {
        // instance boþsa bu scripti ata
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Eðer baþka bir Instance varsa onu yok et
        }
    }

    #region 3DOpponentCard listeli bir þekilde oluþturup GridParent ile gösterme

    public void StartShowOpponentCards()
    {
        // Tüm futbolcularý gridden göster
        if (opponentFootballerList.Count > 0)
        {
            ShowRandomOpponentGrid();
        }
        else
        {
            Debug.LogWarning("Rakip için gösterilecek futbolcu bilgisi bulunamadý.");
        }
    }

    /// <summary>
    /// Rastgele 7 futbolcuyu seçip grid þeklinde gösterir.
    /// </summary>
    private void ShowRandomOpponentGrid()
    {
        // Rastgele 7 futbolcu seç
        List<FootballerInfo> randomFootballers = GetRandomFootballers(opponentFootballerList, 7);

        // Seçilen futbolcularý grid þeklinde göster
        CreateOpponentGrid(randomFootballers);
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

    /// <summary>
    /// Belirtilen futbolcu listesini grid þeklinde gösterir.
    /// </summary>
    private void CreateOpponentGrid(List<FootballerInfo> footballers)
    {
        // Grid için bir parent oluþtur
        GameObject parent = new GameObject("OpponentGridParent");
        parent.transform.SetParent(opponentCardsParent);

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = ForList3dopponentCardPrefab.transform.localPosition;
        Quaternion prefabRotation = ForList3dopponentCardPrefab.transform.localRotation;

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
            ShowOpponentCardDetails(card, footballers[i]);
        }
    }

    private void AddCardClickHandler(GameObject card)
    {
        card.AddComponent<BoxCollider>(); // Týklama algýlamasý için collider ekle
        card.AddComponent<CardClickHandler>(); // Týklama iþlemini yönetecek script ekle
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
                Debug.LogWarning("En fazla 3 kart seçebilirsiniz.");
                return;
            }

            selectedCards.Add(card);
            renderer.material.color = selectedColor;
        }
    }

    /// <summary>
    /// Bilgisayarýn korunacak 3 kartýný rastgele seçip JSON dosyasýna kaydeder.
    /// </summary>
    public void SaveAIProtectedCardsToJson()
    {
        // 7 karttan 3 tane rastgele seç
        List<FootballerInfo> protectedCards = GetRandomFootballers(opponentFootballerList, 3);

        // JSON sarýcý sýnýfý
        FootballerInfoListWrapper wrapper = new FootballerInfoListWrapper { footballers = protectedCards };

        // JSON string'e dönüþtür
        string json = JsonUtility.ToJson(wrapper, true);

        // Dosya yolunu oluþtur
        string filePath = Path.Combine(Application.persistentDataPath, "aiProtectedCard.json");

        try
        {
            // JSON'u dosyaya yaz
            File.WriteAllText(filePath, json);
            Debug.Log($"Bilgisayarýn koruduðu kartlar kaydedildi: {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"JSON dosyasý kaydedilemedi: {ex.Message}");
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
            Debug.LogError("Hiç kart seçilmedi. Seçim iþlemini kontrol edin.");
            return;
        }

        // Seçilen kartlarýn isimlerini al
        List<string> selectedCardNames = new List<string>();
        foreach (GameObject card in selectedCards)
        {
            if (card == null)
            {
                Debug.LogWarning("Bir kart referansý null. Atlanýyor.");
                continue;
            }

            TextMeshPro nameText = card.transform.Find("NameText")?.GetComponent<TextMeshPro>();
            if (nameText != null)
            {
                string cardName = nameText.text.Trim();
                if (!string.IsNullOrEmpty(cardName))
                {
                    selectedCardNames.Add(cardName);
                    Debug.Log($"Seçilen kart ismi alýndý: {cardName}");
                }
                else
                {
                    Debug.LogWarning("Kartýn ismi boþ.");
                }
            }
            else
            {
                Debug.LogError("Kart üzerinde NameText bileþeni bulunamadý.");
            }
        }

        if (selectedCardNames.Count == 0)
        {
            Debug.LogError("Seçilen kartlar arasýnda isim bilgisi alýnamadý.");
            return;
        }

        // Ýsimlere göre opponentFootballerList'ten futbolcularý bul
        foreach (string cardName in selectedCardNames)
        {
            FootballerInfo matchedFootballer = opponentFootballerList.Find(f => f.name == cardName);
            if (matchedFootballer != null)
            {
                selectedCardInfo.Add(matchedFootballer);
                Debug.Log($"Seçilen futbolcu bilgisi eklendi: {matchedFootballer.name}");
            }
            else
            {
                Debug.LogWarning($"Seçilen kart ismi ({cardName}) opponentFootballerList'te bulunamadý.");
            }
        }

        if (selectedCardInfo.Count == 0)
        {
            Debug.LogError("Seçili kartlar arasýnda kaydedilecek geçerli futbolcu bulunamadý.");
            return;
        }

        // JSON dosyasýna kaydet
        string json = JsonUtility.ToJson(new FootballerInfoListWrapper { footballers = selectedCardInfo }, true);
        string filePath = Path.Combine(Application.persistentDataPath, "stoleCards.json");

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Seçilen kartlar kaydedildi: {filePath}");
        }
        catch (IOException ex)
        {
            Debug.LogError($"JSON dosyasý kaydedilemedi: {ex.Message}");
        }
    }

    /// <summary>
    /// Kart detaylarýný doldurur.
    /// </summary>
    private void ShowOpponentCardDetails(GameObject card, FootballerInfo footballer)
    {
        // UI öðelerine eriþim
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Kartýn Renderer'ýna eriþim
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileþeni bulunamadý.");
            return;
        }

        // Kartýn texturesini belirleme
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
                Debug.LogError("Bilinmeyen kart türü!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // UI bilgilerini güncelleme
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;

        // Fiyatý biçimlendirerek göstermek
        int price = 0;
        if (int.TryParse(footballer.price, out price))
        {
            priceText.text = FormatPrice(price);
        }
        else
        {
            priceText.text = "Invalid Price";
        }

        // Görselleri yükleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // Görselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }
    }

    /// <summary>
    /// Kartýn texture'ýný ayarlar.
    /// </summary>
    private void SetCardTexture(Renderer renderer, Texture2D texture)
    {
        foreach (Material mat in renderer.materials)
        {
            mat.mainTexture = texture;
        }
    }
    #endregion

    #region Rastgele 1 Futbolcu Seç ve User'a Karþý At
    public void GenerateandShowOpponentCard()
    {
        // Rastgele bir futbolcu seç ve kartýný göster
        if (opponentFootballerList.Count > 0)
        {
            randomFootballer = SelectRandomFootballer();
            ShowOpponentCard(randomFootballer);
        }
        else
        {
            Debug.LogError("Rakip futbolcu listesi boþ!");
        }
    }

    // Rastgele bir futbolcu seçme
    private FootballerInfo SelectRandomFootballer()
    {
        int randomIndex = Random.Range(0, opponentFootballerList.Count);
        return opponentFootballerList[randomIndex];
    }

    // Seçilen futbolcunun kartýný oluþturma ve sahneye yerleþtirme
    private void ShowOpponentCard(FootballerInfo footballer)
    {
        // Kartý oluþtur ve pozisyonlandýr
        opponentCard = Instantiate(opponentCardPrefab, opponentCardPrefab.transform.position, opponentCardPrefab.transform.rotation);

        // Kart bilgilerini doldur
        PopulateCardDetails(opponentCard, footballer);
    }

    // Kart detaylarýný doldurma
    private void PopulateCardDetails(GameObject card, FootballerInfo footballer)
    {
        // UI öðelerine eriþim
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Kartýn Renderer'ýna eriþim
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileþeni bulunamadý.");
            return;
        }

        // Kartýn texturesini belirleme
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
                Debug.LogError("Bilinmeyen kart türü!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // UI bilgilerini güncelleme
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;

        // Fiyatý biçimlendirerek göstermek
        int price = 0;
        if (int.TryParse(footballer.price, out price))
        {
            priceText.text = FormatPrice(price);
        }
        else
        {
            priceText.text = "Invalid Price";
        }

        // Görselleri yükleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // Görselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }
    }

    private string FormatPrice(int price)
    {
        if (price >= 1000000000)
        {
            return (price / 1000000000f).ToString("0.0") + "B€"; // 1B = 1 Billion = 1 Milyar
        }
        else if (price >= 1000000)
        {
            return (price / 1000000f).ToString("0.0") + "M€"; // 1M = 1 Million = 1 Milyon
        }
        else if (price >= 1000)
        {
            return (price / 1000f).ToString("0.0") + "K€"; // 1K = 1 Thousand = 1 Bin
        }
        else
        {
            return price.ToString() + "€"; // Küçük fiyatlar için direkt gösterim
        }
    }

    #endregion

}

// JSON sarýcý sýnýfý
[System.Serializable]
public class FootballerInfoListWrapper
{
    public List<FootballerInfo> footballers;
}

