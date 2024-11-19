using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameModeScreenManager : MonoBehaviour
{
    [Header("Footballer Chose Screen Settings")]

    [SerializeField] private GameObject footballerCardPrefab; // Kart prefabý
    [SerializeField] private Transform footballersParent; // Grid Layout Group içeren parent objesi
    [SerializeField] private TextMeshProUGUI chooseNumber; // Seçim sayýsýný gösteren UI
    [SerializeField] private TextMeshProUGUI continueButton; // Devam butonu
    [SerializeField] GameObject matchScreen;

    [Header("Packages Textures")]
    [SerializeField] Texture2D BronzePackage;
    [SerializeField] Texture2D SilverPackage;
    [SerializeField] Texture2D GoldPackage;

    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText; // Timer'ý gösterecek UI öðesi
    private float elapsedTime = 0f; // Geçen süre
    private bool isMatching = true; // Eþleþme durumu

    // Json veri çekme ve veri kaydetmek için kullanýlan deðiþkenler
    private string jsonFilePath;
    private int choseNum = 0; // Seçilen oyuncu sayýsý
    private const int maxSelections = 7; // Maksimum seçim sayýsý
    private string chosenJsonFilePath; // Seçilen futbolcularýn kaydedileceði dosya yolu
    private List<FootballerInfo> chosenFootballers = new List<FootballerInfo>(); // Seçilen oyuncular

    [SerializeField] TextMeshProUGUI opponentUsername;
    List<string> opponentNames = new List<string>();

    [SerializeField] TextMeshProUGUI lookingText;
    [SerializeField] TextMeshProUGUI matchFoundText;

    [SerializeField] GameObject BackToModeScreen;

    [SerializeField] Button contunieMatchButton;

    void AddOpponentNames()
    {
        // Rakip kullanýcý adlarýný gerçekçi ve global futbol temalý olarak ekle
        string opponentNamesText = @"
JacobKuzscse
HASMES
kUBÝLS21
Speedy99
GameMaster
ShadowHunter
EpicWarrior
PixelNinja
SilentAssassin
CrazyRider
GoalKing
TheStriker
ProMidfielder
DefensiveWall
SoccerStar10
FootballFanatic
PenaltyMaster
UltimateGoalie
DribbleKing
CrossExpert
Futbolista7
ChampionPlayer
Legendary11
KickerPro
TacticGenius
PassMaster99
MatchWinner
FieldCommander
GoldenBootX
FreekickHero";

        // Kullanýcý adlarýný satýrlara böl ve listeye ekle
        string[] names = opponentNamesText.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        foreach (string name in names)
        {
            opponentNames.Add(name.Trim());
        }
    }

    private void Start()
    {
        jsonFilePath = Path.Combine(Application.persistentDataPath, "myfootballers.json");
        chosenJsonFilePath = Path.Combine(Application.persistentDataPath, "chosenfootballers.json");

        LoadAndDisplayFootballers();
        UpdateUI();

        //Random rakip isimlerini belirle
        AddOpponentNames();

        contunieMatchButton.onClick.AddListener(() => {

            ContunieToMatch();
        });
    }

    private void UpdateUI()
    {
        chooseNumber.text = $"{choseNum} / {maxSelections}";
        if (choseNum == maxSelections)
        {
            contunieMatchButton.gameObject.SetActive(true);
        } else
        {
            contunieMatchButton.gameObject.SetActive(false);
        }
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
        TextMeshProUGUI countText = card.transform.Find("CountText").GetComponent<TextMeshProUGUI>();
        RawImage cardBackground = card.GetComponent<RawImage>(); // Kartýn arka planý

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        Button cardSelectButton = card.transform.Find("SelectButton").GetComponent<Button>();

        // UI öðelerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = footballer.price;
        countText.text = $"x{footballer.footballerCount}";

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
            if (!isSelected && choseNum < maxSelections)
            {
                isSelected = true;
                cardBackground.color = selectedColor;
                choseNum++;
                chosenFootballers.Add(footballer); // Seçilen oyuncuyu listeye ekle
            }
            else if (isSelected)
            {
                isSelected = false;
                cardBackground.color = defaultColor;
                choseNum--;
                chosenFootballers.Remove(footballer); // Seçilen oyuncuyu listeden çýkar
            }

            UpdateUI(); // Seçim sayýsýný güncelle
        });
    }

    public void ContunieToMatch()
    {
        if (chosenFootballers.Count == maxSelections)
        {
            // Seçilen futbolcularýn listesini oluþtur
            FootballerInfoList chosenFootballerList = new FootballerInfoList { footballers = chosenFootballers };

            // JSON formatýna dönüþtür
            string json = JsonUtility.ToJson(chosenFootballerList, true);

            // JSON'u belirtilen dosya yoluna yaz
            File.WriteAllText(chosenJsonFilePath, json);

            Debug.Log($"Seçilen oyuncular baþarýyla kaydedildi: {chosenJsonFilePath}");
            Debug.Log($"Kaydedilen JSON: {json}");

            // Match ekranýný aktif et
            matchScreen.gameObject.SetActive(true);
            UserMatchTimer();

            TextMeshProUGUI username = GameObject.Find("usernameText").GetComponent<TextMeshProUGUI>();
            username.text = PlayerPrefs.GetString("username");

            StartCoroutine(ChooseRandomOpponentUsername());

        }
        else
        {
            Debug.LogWarning("7 oyuncu seçmeden devam edemezsiniz!");
        }
    }

    #region Match Settings

    public void CancelMatching()
    {
        StopAllCoroutines(); // Tüm coroutine'leri durdur

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    IEnumerator ChooseRandomOpponentUsername()
    {
        int randomTime = Random.Range(3, 8);
        yield return new WaitForSeconds(randomTime);

        int randIndex = Random.Range(0, opponentNames.Count - 1);
        opponentUsername.text = opponentNames[randIndex];

        PlayerPrefs.SetString("opponentUsername", opponentNames[randIndex]);

        opponentUsername.gameObject.SetActive(true);
        lookingText.gameObject.SetActive(false);
        matchFoundText.gameObject.SetActive(true);
        BackToModeScreen.gameObject.SetActive(false);

        //süreyi durdur
        StopMatchTimer();

        //Eþleþme bulunduðu için kullanýcýyý oyun ekranýna gönder
        StartCoroutine(GoToKaliteGameScreen());
    }

    IEnumerator GoToKaliteGameScreen()
    {
        yield return new WaitForSeconds(2);

        SceneManager.LoadScene(5);
    }

    #endregion

    #region Timer Method
    void UserMatchTimer()
    {
        // Eþleþme baþladýðýnda zamanlayýcýyý baþlat
        isMatching = true;
        elapsedTime = 0f;
        StartCoroutine(UpdateMatchTimer());
    }

    IEnumerator UpdateMatchTimer()
    {
        while (isMatching)
        {
            // Geçen süreyi artýr
            elapsedTime += Time.deltaTime;

            // Süreyi "Dakika:Saniye" formatýnda ayarla
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            yield return null; // Bir sonraki frame'e kadar bekle
        }
    }

    void StopMatchTimer()
    {
        // Zamanlayýcýyý durdur ve ekranda "Eþleþme Tamamlandý" mesajý göster
        isMatching = false;
    }

    #endregion

    public void BacktoMenu()
    {
        SceneManager.LoadScene(1);
    }
}