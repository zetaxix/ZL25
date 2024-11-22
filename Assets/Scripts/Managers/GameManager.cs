using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] TextMeshPro userScoreText;   // Kullan�c� skoru
    [SerializeField] TextMeshPro opponentScoreText; // Rakip skoru

    private int userScore = 0;      // Kullan�c� skoru
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

    private bool matchFinish = false;    // Ma��n bitip bitmedi�ini g�sterir
    private bool matchStatusChecked = false; // Ma� durumu bir kez kontrol edildi mi?

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
            matchStatusChecked = true; // Fonksiyonun bir daha �al��mamas�n� sa�lar.

            Debug.Log($"Ma� Bitti! {PlayerPrefs.GetString("username")} kazand�!!");

            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);

            GameObject trueUserCard = UserCards.transform.Find("GridParent").gameObject;

            trueUserCard.SetActive(false);
            OpponentCards.SetActive(false);

            MatchFinishScreen.SetActive(true);

            if (userScore == 3)
            {
                matchStatusText.text = "Ma�� Kazand�n!";
                matchButton.text = "Kart �almaya Ge� =>";

                PlayerPrefs.SetString("WhoWin", "UserWin");
            }
            else if (opponentScore == 3)
            {
                matchStatusText.text = "Ma�� Kaybettin!";
                matchButton.text = "Kart Korumaya Ge� =>";

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

            TitleStatus.text = "Kart �alma";
            MatchFinishStatus.text = "�almak istedi�iniz 3 tane kart� se�in ve sonuca ge�in.";

            GameScreenOpponentCardManager.Instance.StartShowOpponentCards();

        }
        else if (PlayerPrefs.GetString("WhoWin") == "OpponentWin")
        {
            CardStoleAndSaveScreen.SetActive(true);
            MatchFinishScreen.SetActive(false);

            TitleStatus.text = "Kart Koruma";
            MatchFinishStatus.text = "Korumak istedi�iniz 3 tane kart� se�in ve sonuca ge�in.";

            KaliteKazanirManager.Instance.LoadAndDisplayFootballersForProtectedScreen();
        }
    }   
    public IEnumerator CompareAndAddScore()
    {
        yield return new WaitForSeconds(CardMovement.instance.randTime + 1.10f);

        // Kullan�c� ve rakip puanlar�n� al
        int userRating = int.Parse(UserFootballerRating());
        int opponentRating = int.Parse(OpponentFootballerRating());

        Debug.Log($"User Rating: {userRating}, Opponent Rating: {opponentRating}");

        yield return new WaitForSeconds(1.5f);

        // Skorlar� kar��la�t�r ve ekle
        if (userRating > opponentRating)
        {
            if (userScore < 3) // Kullan�c� skoru 3�ten k���kse art�r
            {
                userScore++;
                userScoreText.text = userScore.ToString(); // UI g�ncelle
                Debug.Log("Kullan�c� skoru art�r�ld�!");

                // Kartlar� tekrar etkile�ime a�
                CardMovement.instance.UnLockAllCards();

                // Rakip kart objesini al ve yok et
                if (GameScreenOpponentCardManager.Instance.opponentCard != null)
                {
                    GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                    Destroy(opponentCardObject); // Rakip kart� yok et
                    GameScreenOpponentCardManager.Instance.opponentCard = null;
                }

                // Kullan�c� kart�n� yok et
                if (CardMovement.instance.rememberSelectedCard != null)
                {
                    // Kart� yok etmeden �nce listeden ��kar
                    CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                    Destroy(CardMovement.instance.rememberSelectedCard); // Kullan�c�n�n se�ti�i kart� yok et
                    CardMovement.instance.rememberSelectedCard = null;  // Kart referans�n� s�f�rla
                }
            }
        }
        else if (opponentRating > userRating)
        {
            if (opponentScore < 3) // Rakip skoru 3�ten k���kse art�r
            {
                opponentScore++;
                opponentScoreText.text = opponentScore.ToString(); // UI g�ncelle
                Debug.Log("Rakip skoru art�r�ld�!");

                // Kartlar� tekrar etkile�ime a�
                CardMovement.instance.UnLockAllCards();

                // Rakip kart objesini al ve yok et
                if (GameScreenOpponentCardManager.Instance.opponentCard != null)
                {
                    GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                    Destroy(opponentCardObject); // Rakip kart� yok et
                    GameScreenOpponentCardManager.Instance.opponentCard = null;
                }

                if (CardMovement.instance.rememberSelectedCard != null)
                {
                    // Kart� yok etmeden �nce listeden ��kar
                    CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                    Destroy(CardMovement.instance.rememberSelectedCard); // Kullan�c�n�n se�ti�i kart� yok et
                    CardMovement.instance.rememberSelectedCard = null;  // Kart referans�n� s�f�rla
                }
            }
        }
        else if (userRating == opponentRating)
        {
            // Kartlar� tekrar etkile�ime a�
            CardMovement.instance.UnLockAllCards();

            // Rakip kart objesini al ve yok et
            if (GameScreenOpponentCardManager.Instance.opponentCard != null)
            {
                GameObject opponentCardObject = GameScreenOpponentCardManager.Instance.opponentCard;
                Destroy(opponentCardObject); // Rakip kart� yok et
                GameScreenOpponentCardManager.Instance.opponentCard = null;
            }

            if (CardMovement.instance.rememberSelectedCard != null)
            {
                // Kart� yok etmeden �nce listeden ��kar
                CardMovement.instance.allCards.Remove(CardMovement.instance.rememberSelectedCard);
                Destroy(CardMovement.instance.rememberSelectedCard); // Kullan�c�n�n se�ti�i kart� yok et
                CardMovement.instance.rememberSelectedCard = null;  // Kart referans�n� s�f�rla
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
            Debug.LogError("Hata: Se�ilen kart null. Bir kart se�ili de�il.");
            return "0";
        }

        Transform ratingTextTransform = CardMovement.instance.rememberSelectedCard.transform.Find("RatingText");
        if (ratingTextTransform == null)
        {
            Debug.LogError($"Hata: {CardMovement.instance.rememberSelectedCard.name} kart�nda 'RatingText' isimli obje bulunamad�.");
            return "0";
        }

        TextMeshPro selectedCardText = ratingTextTransform.GetComponent<TextMeshPro>();
        if (selectedCardText == null)
        {
            Debug.LogError("Hata: 'RatingText' objesinde TextMeshProUGUI bile�eni yok.");
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