using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] TextMeshPro userScoreText;   // Kullanýcý skoru
    [SerializeField] TextMeshPro opponentScoreText; // Rakip skoru

    private int userScore = 0;      // Kullanýcý skoru
    private int opponentScore = 0; // Rakip skoru

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

    private bool matchFinish = false;    // Maçýn bitip bitmediðini gösterir
    private bool matchStatusChecked = false; // Maç durumu bir kez kontrol edildi mi?

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

    private void Update()
    {
        if (!matchStatusChecked && (userScore == 3 || opponentScore == 3))
        {
            matchFinish = true;
            MatchStatusCheck();
        }
    }

    private void MatchStatusCheck()
    {
        if (matchFinish && !matchStatusChecked)
        {
            matchStatusChecked = true; // Fonksiyonun bir daha çalýþmamasýný saðlar.

            Debug.Log($"Maç Bitti! {PlayerPrefs.GetString("username")} kazandý!!");

            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);

            GameObject trueUserCard = UserCards.transform.Find("GridParent").gameObject;

            trueUserCard.SetActive(false);
            OpponentCards.SetActive(false);

            MatchFinishScreen.SetActive(true);

            if (userScore == 3)
            {
                matchStatusText.text = "Maçý Kazandýn!";
                matchButton.text = "Kart Çalmaya Geç =>";

                PlayerPrefs.SetString("WhoWin", "UserWin");
            }
            else if (opponentScore == 3)
            {
                matchStatusText.text = "Maçý Kaybettin!";
                matchButton.text = "Kart Korumaya Geç =>";

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

            TitleStatus.text = "Kart Çalma";
            MatchFinishStatus.text = "Çalmak istediðiniz 3 tane kartý seçin ve sonuca geçin.";

            GameScreenOpponentCardManager.Instance.StartShowOpponentCards();

        }
        else if (PlayerPrefs.GetString("WhoWin") == "OpponentWin")
        {
            CardStoleAndSaveScreen.SetActive(true);
            MatchFinishScreen.SetActive(false);

            TitleStatus.text = "Kart Koruma";
            MatchFinishStatus.text = "Korumak istediðiniz 3 tane kartý seçin ve sonuca geçin.";

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