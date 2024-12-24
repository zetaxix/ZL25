using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Scoreboard Settings")]
    [SerializeField] Animator ScoreboardAnim;
    [SerializeField] GameObject Scoreboard;
    [SerializeField] TextMeshProUGUI userScoreText;   // Kullanýcý skoru
    [SerializeField] TextMeshProUGUI opponentScoreText; // Rakip skoru
    [SerializeField] TextMeshProUGUI timerText; // Rakip skoru

    [Header("Cards Objects")]
    [SerializeField] GameObject UserCards;
    [SerializeField] GameObject OpponentCards;

    [Header("Match Finish Screen Settings")]
    [SerializeField] GameObject MatchFinishScreen;
    [SerializeField] TextMeshPro matchStatusText;
    [SerializeField] TextMeshProUGUI matchButton;

    [Header("Card Stole And Save Screen")]
    [SerializeField] GameObject CardStoleAndSaveScreen;
    [SerializeField] TextMeshPro TitleStatus;
    [SerializeField] TextMeshPro MatchFinishStatus;
    [SerializeField] TextMeshPro ResultButton;

    // Match Check
    private bool matchFinish = false;    // Maçýn bitip bitmediðini gösterir
    private bool matchStatusChecked = false; // Maç durumu bir kez kontrol edildi mi?

    // Initialize Score
    private int userScore = 0;      // Kullanýcý skoru
    private int opponentScore = 0; // Rakip skoru

    //Timer Settings
    private float elapsedTime = 0f; // Geçen süre
    private bool isRunning = false; // Timer çalýþýyor mu?

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        //Scoreboard Anim and Timer Started
        StartCoroutine( StartScoreboardAnim() );
    }

    private void Update()
    {
        if (!matchStatusChecked && (userScore == 3 || opponentScore == 3))
        {
            matchFinish = true;
            MatchStatusCheck();
        }

        if (isRunning)
        {
            // Geçen süreyi artýr
            elapsedTime += Time.deltaTime;

            // Zamaný uygun formatta yazdýr
            UpdateTimerText();
        }
    }

    #region Scoreboard Animation

    IEnumerator StartScoreboardAnim()
    {
        yield return new WaitForSeconds(2.30f);
        ScoreboardAnim.SetTrigger("MatchStarted");

        //Scoreboard timer started
        StartTimer();
    }

    #endregion

    #region Timer Settings

    // Timer'ý baþlatan fonksiyon
    public void StartTimer()
    {
        isRunning = true;
        elapsedTime = 0f; // Zamaný sýfýrla
    }

    // Timer'ý durduran fonksiyon
    public void StopTimer()
    {
        isRunning = false;
    }

    // Timer'ý sýfýrlayan fonksiyon
    public void ResetTimer()
    {
        elapsedTime = 0f;
        UpdateTimerText();
    }

    // Timer'ýn TextMeshPro'ya yazdýrýlmasýný saðlayan fonksiyon
    private void UpdateTimerText()
    {
        // Elapsed time'ý dakikalar ve saniyelere böl
        int minutes = Mathf.FloorToInt(elapsedTime / 60f); // Dakikayý hesapla
        int seconds = Mathf.FloorToInt(elapsedTime % 60f); // Saniyeyi hesapla

        // TextMeshPro'da uygun formatta göster
        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    #endregion

    private void MatchStatusCheck()
    {
        if (matchFinish && !matchStatusChecked)
        {
            matchStatusChecked = true; // Fonksiyonun bir daha çalýþmamasýný saðlar.

            Debug.Log($"Maç Bitti! {PlayerPrefs.GetString("username")} win!");

            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);

            GameObject trueUserCard = UserCards.transform.Find("GridParent").gameObject;

            trueUserCard.SetActive(false);
            OpponentCards.SetActive(false);

            MatchFinishScreen.SetActive(true);
            Scoreboard.SetActive(false);

            StopTimer();

            if (userScore == 3)
            {
                matchStatusText.text = "You Win!";
                matchButton.text = "Continue";

                // matchButton'un içindeki TextMeshPro bileþenine eriþim
                TextMeshProUGUI matchStatusTextComp = matchStatusText.GetComponentInChildren<TextMeshProUGUI>();

                if (matchStatusTextComp != null)
                {
                    matchStatusTextComp.color = Color.green; // Metin rengini yeþil yap
                }
                else
                {
                    Debug.LogWarning("matchButton üzerinde TextMeshProUGUI bileþeni bulunamadý.");
                }

                PlayerPrefs.SetString("WhoWin", "UserWin");
            }
            else if (opponentScore == 3)
            {
                matchStatusText.text = "You Lost!";
                matchButton.text = "Continue";

                // matchButton'un içindeki TextMeshPro bileþenine eriþim
                TextMeshProUGUI matchStatusTextComp = matchStatusText.GetComponentInChildren<TextMeshProUGUI>();

                if (matchStatusTextComp != null)
                {
                    matchStatusTextComp.color = Color.red; // Metin rengini yeþil yap
                }
                else
                {
                    Debug.LogWarning("matchButton üzerinde TextMeshProUGUI bileþeni bulunamadý.");
                }

                PlayerPrefs.SetString("WhoWin", "OpponentWin");

            }
        }
    }

    public void GoToStoleorSaveScreen()
    {
        if (PlayerPrefs.GetString("WhoWin") == "UserWin")
        {
            CardStoleAndSaveScreen.SetActive(true);
            MatchFinishScreen.SetActive(false);

            TitleStatus.text = "Card Stealing";
            MatchFinishStatus.text = "Select the 3 cards you want to steal and go to the result screen.";

            GameScreenOpponentCardManager.Instance.StartShowOpponentCards();

        }
        else if (PlayerPrefs.GetString("WhoWin") == "OpponentWin")
        {
            CardStoleAndSaveScreen.SetActive(true);
            MatchFinishScreen.SetActive(false);

            TitleStatus.text = "Card Protection";
            MatchFinishStatus.text = "Select the 3 cards you want to protect and go to the result screen.";

            KaliteKazanirManager.Instance.LoadAndDisplayFootballersForProtectedScreen();
        }
    }   
    public IEnumerator CompareAndAddScore()
    {
        yield return new WaitForSeconds(CardMovement.instance.randTime + 1.10f);

        // Kullanýcý ve rakip puanlarýný al
        int userRating = int.Parse(UserFootballerRating());
        int opponentRating = int.Parse(OpponentFootballerRating());

        Debug.Log($"User Rating: {userRating}, Opponent Rating: {opponentRating}");

        yield return new WaitForSeconds(1.5f);

        // Skorlarý karþýlaþtýr ve ekle
        if (userRating > opponentRating)
        {
            if (userScore < 3) // Kullanýcý skoru 3’ten küçükse artýr
            {
                userScore++;
                userScoreText.text = userScore.ToString(); // UI güncelle
                Debug.Log("Kullanýcý skoru artýrýldý!");

                // Kartlarý tekrar etkileþime aç
                CardMovement.instance.UnLockAllCards();

                // Rakip kart objesini al ve yok et
                if (GameScreenOpponentCardManager.Instance.opponentCard != null)
                {
                    GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                    Destroy(opponentCardObject); // Rakip kartý yok et
                    GameScreenOpponentCardManager.Instance.opponentCard = null;
                }

                // Kullanýcý kartýný yok et
                if (CardMovement.instance.rememberSelectedCard != null)
                {
                    // Kartý yok etmeden önce listeden çýkar
                    CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                    Destroy(CardMovement.instance.rememberSelectedCard); // Kullanýcýnýn seçtiði kartý yok et
                    CardMovement.instance.rememberSelectedCard = null;  // Kart referansýný sýfýrla
                }
            }
        }
        else if (opponentRating > userRating)
        {
            if (opponentScore < 3) // Rakip skoru 3’ten küçükse artýr
            {
                opponentScore++;
                opponentScoreText.text = opponentScore.ToString(); // UI güncelle
                Debug.Log("Rakip skoru artýrýldý!");

                // Kartlarý tekrar etkileþime aç
                CardMovement.instance.UnLockAllCards();

                // Rakip kart objesini al ve yok et
                if (GameScreenOpponentCardManager.Instance.opponentCard != null)
                {
                    GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                    Destroy(opponentCardObject); // Rakip kartý yok et
                    GameScreenOpponentCardManager.Instance.opponentCard = null;
                }

                if (CardMovement.instance.rememberSelectedCard != null)
                {
                    // Kartý yok etmeden önce listeden çýkar
                    CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                    Destroy(CardMovement.instance.rememberSelectedCard); // Kullanýcýnýn seçtiði kartý yok et
                    CardMovement.instance.rememberSelectedCard = null;  // Kart referansýný sýfýrla
                }
            }
        }
        else if (userRating == opponentRating)
        {
            // Kartlarý tekrar etkileþime aç
            CardMovement.instance.UnLockAllCards();

            // Rakip kart objesini al ve yok et
            if (GameScreenOpponentCardManager.Instance.opponentCard != null)
            {
                GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                Destroy(opponentCardObject); // Rakip kartý yok et
                GameScreenOpponentCardManager.Instance.opponentCard = null;
            }

            if (CardMovement.instance.rememberSelectedCard != null)
            {
                // Kartý yok etmeden önce listeden çýkar
                CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                Destroy(CardMovement.instance.rememberSelectedCard); // Kullanýcýnýn seçtiði kartý yok et
                CardMovement.instance.rememberSelectedCard = null;  // Kart referansýný sýfýrla
            }
        }
    }

    public string UserFootballerRating()
    {
        if (CardMovement.instance == null)
        {
            Debug.LogError("Hata: CardMovement instance null.");
            return "0";
        }

        if (CardMovement.instance.rememberSelectedCard == null)
        {
            Debug.LogError("Hata: Seçilen kart null. Bir kart seçili deðil.");
            return "0";
        }

        Transform ratingTextTransform = CardMovement.instance.rememberSelectedCard.transform.Find("RatingText");
        if (ratingTextTransform == null)
        {
            Debug.LogError($"Hata: {CardMovement.instance.rememberSelectedCard.name} kartýnda 'RatingText' isimli obje bulunamadý.");
            return "0";
        }

        TextMeshPro selectedCardText = ratingTextTransform.GetComponent<TextMeshPro>();
        if (selectedCardText == null)
        {
            Debug.LogError("Hata: 'RatingText' objesinde TextMeshProUGUI bileþeni yok.");
            return "0";
        }

        return selectedCardText.text;
    }

    public string OpponentFootballerRating()
    {
        if (GameScreenOpponentCardManager.Instance == null)
        {
            Debug.LogError("Hata: GameScreenOpponentCardManager instance null.");
            return "0";
        }

        return GameScreenOpponentCardManager.Instance.randomFootballer.rating;
    }

    public void UserGoToMenu()
    {
        SceneManager.LoadScene(1);
    }
}