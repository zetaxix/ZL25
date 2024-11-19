using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] TextMeshPro userScoreText;   // Kullan�c� skoru
    [SerializeField] TextMeshPro opponentScoreText; // Rakip skoru

    private int userScore = 0;      // Kullan�c� skoru
    private int opponentScore = 0; // Rakip skoru

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

    private void Update()
    {
        if (userScore == 3)
        {
            Debug.Log($"Ma� Bitti {PlayerPrefs.GetString("username")} Kazand�!!");

            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);
        }
        else if (opponentScore == 3)
        {
            Debug.Log($"Ma� Bitti {PlayerPrefs.GetString("opponentUsername")} Kazand�!!");
            
            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);
        }
    }

    public IEnumerator CompareAndAddScore()
    {
        // Kullan�c� ve rakip puanlar�n� al
        int userRating = int.Parse(UserFootballerRating());
        int opponentRating = int.Parse(OpponentFootballerRating());

        Debug.Log($"User Rating: {userRating}, Opponent Rating: {opponentRating}");

        yield return new WaitForSeconds(2f);

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

        return GameScreenOpponentCardManager.Instance.randSelectFootballer.rating;
    }
}