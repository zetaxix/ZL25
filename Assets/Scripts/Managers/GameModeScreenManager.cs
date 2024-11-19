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

    [SerializeField] private GameObject footballerCardPrefab; // Kart prefab�
    [SerializeField] private Transform footballersParent; // Grid Layout Group i�eren parent objesi
    [SerializeField] private TextMeshProUGUI chooseNumber; // Se�im say�s�n� g�steren UI
    [SerializeField] private TextMeshProUGUI continueButton; // Devam butonu
    [SerializeField] GameObject matchScreen;

    [Header("Packages Textures")]
    [SerializeField] Texture2D BronzePackage;
    [SerializeField] Texture2D SilverPackage;
    [SerializeField] Texture2D GoldPackage;

    [Header("Timer Settings")]
    [SerializeField] private TextMeshProUGUI timerText; // Timer'� g�sterecek UI ��esi
    private float elapsedTime = 0f; // Ge�en s�re
    private bool isMatching = true; // E�le�me durumu

    // Json veri �ekme ve veri kaydetmek i�in kullan�lan de�i�kenler
    private string jsonFilePath;
    private int choseNum = 0; // Se�ilen oyuncu say�s�
    private const int maxSelections = 7; // Maksimum se�im say�s�
    private string chosenJsonFilePath; // Se�ilen futbolcular�n kaydedilece�i dosya yolu
    private List<FootballerInfo> chosenFootballers = new List<FootballerInfo>(); // Se�ilen oyuncular

    [SerializeField] TextMeshProUGUI opponentUsername;
    List<string> opponentNames = new List<string>();

    [SerializeField] TextMeshProUGUI lookingText;
    [SerializeField] TextMeshProUGUI matchFoundText;

    [SerializeField] GameObject BackToModeScreen;

    [SerializeField] Button contunieMatchButton;

    void AddOpponentNames()
    {
        // Rakip kullan�c� adlar�n� ger�ek�i ve global futbol temal� olarak ekle
        string opponentNamesText = @"
JacobKuzscse
HASMES
kUB�LS21
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

        // Kullan�c� adlar�n� sat�rlara b�l ve listeye ekle
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

        Button cardSelectButton = card.transform.Find("SelectButton").GetComponent<Button>();

        // UI ��elerini doldur
        nameText.text = footballer.name;
        ratingText.text = footballer.rating;
        priceText.text = footballer.price;
        countText.text = $"x{footballer.footballerCount}";

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
            if (!isSelected && choseNum < maxSelections)
            {
                isSelected = true;
                cardBackground.color = selectedColor;
                choseNum++;
                chosenFootballers.Add(footballer); // Se�ilen oyuncuyu listeye ekle
            }
            else if (isSelected)
            {
                isSelected = false;
                cardBackground.color = defaultColor;
                choseNum--;
                chosenFootballers.Remove(footballer); // Se�ilen oyuncuyu listeden ��kar
            }

            UpdateUI(); // Se�im say�s�n� g�ncelle
        });
    }

    public void ContunieToMatch()
    {
        if (chosenFootballers.Count == maxSelections)
        {
            // Se�ilen futbolcular�n listesini olu�tur
            FootballerInfoList chosenFootballerList = new FootballerInfoList { footballers = chosenFootballers };

            // JSON format�na d�n��t�r
            string json = JsonUtility.ToJson(chosenFootballerList, true);

            // JSON'u belirtilen dosya yoluna yaz
            File.WriteAllText(chosenJsonFilePath, json);

            Debug.Log($"Se�ilen oyuncular ba�ar�yla kaydedildi: {chosenJsonFilePath}");
            Debug.Log($"Kaydedilen JSON: {json}");

            // Match ekran�n� aktif et
            matchScreen.gameObject.SetActive(true);
            UserMatchTimer();

            TextMeshProUGUI username = GameObject.Find("usernameText").GetComponent<TextMeshProUGUI>();
            username.text = PlayerPrefs.GetString("username");

            StartCoroutine(ChooseRandomOpponentUsername());

        }
        else
        {
            Debug.LogWarning("7 oyuncu se�meden devam edemezsiniz!");
        }
    }

    #region Match Settings

    public void CancelMatching()
    {
        StopAllCoroutines(); // T�m coroutine'leri durdur

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

        //s�reyi durdur
        StopMatchTimer();

        //E�le�me bulundu�u i�in kullan�c�y� oyun ekran�na g�nder
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
        // E�le�me ba�lad���nda zamanlay�c�y� ba�lat
        isMatching = true;
        elapsedTime = 0f;
        StartCoroutine(UpdateMatchTimer());
    }

    IEnumerator UpdateMatchTimer()
    {
        while (isMatching)
        {
            // Ge�en s�reyi art�r
            elapsedTime += Time.deltaTime;

            // S�reyi "Dakika:Saniye" format�nda ayarla
            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);
            timerText.text = $"{minutes:00}:{seconds:00}";

            yield return null; // Bir sonraki frame'e kadar bekle
        }
    }

    void StopMatchTimer()
    {
        // Zamanlay�c�y� durdur ve ekranda "E�le�me Tamamland�" mesaj� g�ster
        isMatching = false;
    }

    #endregion

    public void BacktoMenu()
    {
        SceneManager.LoadScene(1);
    }
}