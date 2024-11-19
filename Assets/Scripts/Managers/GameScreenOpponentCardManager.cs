using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameScreenOpponentCardManager : MonoBehaviour
{
    public static GameScreenOpponentCardManager Instance;

    [Header("Opponent Card Prefab")]
    [SerializeField] private GameObject opponentCardPrefab; // Rakip kart prefab'�
    public GameObject opponentCard;

    [Header("Card Textures")]
    [SerializeField] private Texture2D BronzeCardTexture;
    [SerializeField] private Texture2D SilverCardTexture;
    [SerializeField] private Texture2D GoldCardTexture;

    [Header("Opponent Footballer List")]
    [SerializeField] private List<FootballerInfo> opponentFootballerList = new List<FootballerInfo>();

    public FootballerInfo randSelectFootballer;

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

    public void GenerateandShowOpponentCard()
    {
        // Rastgele bir futbolcu se� ve kart�n� g�ster
        if (opponentFootballerList.Count > 0)
        {
            FootballerInfo randomFootballer = SelectRandomFootballer();
            ShowOpponentCard(randomFootballer);

            randSelectFootballer = SelectRandomFootballer();
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

}