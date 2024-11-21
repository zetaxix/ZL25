using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro; // List kullanýmý için

public class CardMovement : MonoBehaviour
{
    public static CardMovement instance;

    public GameObject selectedCard;  // Þu anda sürüklenen kart
    public GameObject rememberSelectedCard; // Son seçilen kart (býrakýldýktan sonra da saklanýr)

    private bool isDragging = false; // Kartýn sürüklenme durumu
    private bool isLocked = false;   // Kart yerleþtikten sonra diðer kartlarý kilitlemek için

    [SerializeField] private float minZPosition = -9.058f;  // Z pozisyonu için alt sýnýr
    [SerializeField] private float maxZPosition = -8.93532f;  // Z pozisyonu için üst sýnýr
    [SerializeField] private float minXPosition = -0.3306499f;  // X pozisyonu için alt sýnýr
    [SerializeField] private float maxXPosition = 0.3306499f;  // X pozisyonu için üst sýnýr
    [SerializeField] private float fixedCardYPosition = 0.6555f; // Kartýn sabitleneceði Y pozisyonu

    public List<GameObject> allCards; // Tüm kartlarýn listesi

    public GameObject cardAreaElements;
    [SerializeField] Transform opponentCards;

    private int cardIndex = 1;  // Ýlk kart olan 3DCardForGameScreen1'den baþlar
    public int randTime;

    void Awake()
    {
        // instance boþsa bu scripti ata
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // Eðer baþka bir instance varsa onu yok et
        }
    }

    void Start()
    {
        allCards = new List<GameObject>(GameObject.FindGameObjectsWithTag("Card"));

        StartCoroutine(CardAreaElementFade());
    }

    void Update()
    {
        // Eðer kartlar kilitliyse daha fazla iþlem yapýlmaz
        if (isLocked) return;

        // Sol týklama baþladýðýnda
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Card"))
                {
                    // Sürüklemek için kartý seç
                    selectedCard = hit.collider.gameObject;
                    rememberSelectedCard = selectedCard; // Son seçilen kartý kaydet
                    isDragging = true;
                }
            }
        }

        // Kart sürükleniyorsa
        if (isDragging && selectedCard != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Yeni pozisyonlarý hesapla
                float newZPosition = Mathf.Clamp(hit.point.z, minZPosition, maxZPosition);
                float newXPosition = Mathf.Clamp(hit.point.x, minXPosition, maxXPosition);

                // Kartýn pozisyonunu güncelle
                selectedCard.transform.position = new Vector3(newXPosition, selectedCard.transform.position.y, newZPosition);

                // Eðer CardArea'ya çarptýysa
                if (Physics.Raycast(selectedCard.transform.position, Vector3.down, out RaycastHit areaHit))
                {
                    if (areaHit.collider.CompareTag("CardArea"))
                    {
                        // Kartý CardArea'nýn pozisyonuna sabitle ve Y pozisyonunu ayarla
                        selectedCard.transform.position = new Vector3(areaHit.collider.transform.position.x, fixedCardYPosition,
                            areaHit.collider.transform.position.z
                        );

                        isDragging = false;  // Sürüklemeyi bitir
                        selectedCard = null;  // Kart seçimini kaldýr

                        LockAllCards(); // Tüm kartlarýn etkileþimini kapat

                        cardAreaElements.SetActive(false);

                        // BURADA OpponentCards OBJELERÝNDEN BÝRÝNÝ SÝL
                        if (opponentCards != null)
                        {
                            // Kartlarý sýrayla silmek için kartIndex kullanýyoruz
                            string cardToDeleteName = "3DCardForGameScreen" + cardIndex;

                            // OpponentCards içindeki belirli kartý buluyoruz
                            Transform cardToDelete = opponentCards.Find(cardToDeleteName);
                            if (cardToDelete != null)
                            {
                                Destroy(cardToDelete.gameObject); // GameObject'i sil
                                cardIndex++; // Sonraki kartý silmek için sayaç artýrýlýr
                            }
                            else
                            {
                                Debug.Log("Silinmesi gereken kart bulunamadý.");
                            }
                        }
                        else
                        {
                            Debug.LogError("OpponentCards objesi bulunamadý.");
                        }

                        StartCoroutine(GenerateOpponentCardandDelay());

                        StartCoroutine(GameManager.Instance.CompareAndAddScore());
                    }
                }
            }
        }
    }

    public IEnumerator GenerateOpponentCardandDelay()
    {
        randTime = Random.Range(1, 4);

        yield return new WaitForSeconds(randTime);
        GameScreenOpponentCardManager.Instance.GenerateandShowOpponentCard();
        Opponent3DStartAnim.instance.StartAnim();
    }

    //Tüm kartlarý kilitle
    public void LockAllCards()
    {
        isLocked = true; // Kartlarý kilitle
        allCards.RemoveAll(card => card == null); // Yok edilmiþ kartlarý listeden çýkar

        foreach (GameObject card in allCards)
        {
            if (card != null && card.TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = false; // Kartlarýn collider'larýný kapat
            }
        }
    }

    // Tüm kartlarýn kilidini aç
    public void UnLockAllCards()
    {
        isLocked = false; // Kartlarýn kilidini aç
        allCards.RemoveAll(card => card == null); // Yok edilmiþ kartlarý listeden çýkar

        foreach (GameObject card in allCards)
        {
            if (card != null && card.TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = true; // Kartlarýn collider'larýný aç
            }
        }

        cardAreaElements.SetActive(true); // Kart alanýný görünür yap
    }


    IEnumerator CardAreaElementFade()
    {
        yield return new WaitForSeconds(2.5f);
        cardAreaElements.SetActive(true);
    }
}