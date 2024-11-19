using System.Collections;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] TextMeshPro userScoreText;   // Kullanýcý skoru
    [SerializeField] TextMeshPro opponentScoreText; // Rakip skoru

    private int userScore = 0;      // Kullanýcý skoru
    private int opponentScore = 0; // Rakip skoru

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

    private void Update()
    {
        if (userScore == 3)
        {
            Debug.Log($"Maç Bitti {PlayerPrefs.GetString("username")} Kazandý!!");

            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);
        }
        else if (opponentScore == 3)
        {
            Debug.Log($"Maç Bitti {PlayerPrefs.GetString("opponentUsername")} Kazandý!!");
            
            CardMovement.instance.LockAllCards();
            CardMovement.instance.cardAreaElements.SetActive(false);
        }
    }

    public IEnumerator CompareAndAddScore()
    {
        // Kullanýcý ve rakip puanlarýný al
        int userRating = int.Parse(UserFootballerRating());
        int opponentRating = int.Parse(OpponentFootballerRating());

        Debug.Log($"User Rating: {userRating}, Opponent Rating: {opponentRating}");

        yield return new WaitForSeconds(2f);

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

        return GameScreenOpponentCardManager.Instance.randSelectFootballer.rating;
    }
}