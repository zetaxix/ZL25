using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro; // List kullan�m� i�in

public class CardMovement : MonoBehaviour
{
    public static CardMovement instance;

    public GameObject selectedCard;  // �u anda s�r�klenen kart
    public GameObject rememberSelectedCard; // Son se�ilen kart (b�rak�ld�ktan sonra da saklan�r)

    private bool isDragging = false; // Kart�n s�r�klenme durumu
    private bool isLocked = false;   // Kart yerle�tikten sonra di�er kartlar� kilitlemek i�in

    [SerializeField] private float minZPosition = -9.058f;  // Z pozisyonu i�in alt s�n�r
    [SerializeField] private float maxZPosition = -8.93532f;  // Z pozisyonu i�in �st s�n�r
    [SerializeField] private float minXPosition = -0.3306499f;  // X pozisyonu i�in alt s�n�r
    [SerializeField] private float maxXPosition = 0.3306499f;  // X pozisyonu i�in �st s�n�r
    [SerializeField] private float fixedCardYPosition = 0.6555f; // Kart�n sabitlenece�i Y pozisyonu

    public List<GameObject> allCards; // T�m kartlar�n listesi

    public GameObject cardAreaElements;
    [SerializeField] Transform opponentCards;

    private int cardIndex = 1;  // �lk kart olan 3DCardForGameScreen1'den ba�lar
    public int randTime;

    void Awake()
    {
        // instance bo�sa bu scripti ata
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject); // E�er ba�ka bir instance varsa onu yok et
        }
    }

    void Start()
    {
        allCards = new List<GameObject>(GameObject.FindGameObjectsWithTag("Card"));

        StartCoroutine(CardAreaElementFade());
    }

    void Update()
    {
        // E�er kartlar kilitliyse daha fazla i�lem yap�lmaz
        if (isLocked) return;

        // Sol t�klama ba�lad���nda
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Card"))
                {
                    // S�r�klemek i�in kart� se�
                    selectedCard = hit.collider.gameObject;
                    rememberSelectedCard = selectedCard; // Son se�ilen kart� kaydet
                    isDragging = true;
                }
            }
        }

        // Kart s�r�kleniyorsa
        if (isDragging && selectedCard != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // Yeni pozisyonlar� hesapla
                float newZPosition = Mathf.Clamp(hit.point.z, minZPosition, maxZPosition);
                float newXPosition = Mathf.Clamp(hit.point.x, minXPosition, maxXPosition);

                // Kart�n pozisyonunu g�ncelle
                selectedCard.transform.position = new Vector3(newXPosition, selectedCard.transform.position.y, newZPosition);

                // E�er CardArea'ya �arpt�ysa
                if (Physics.Raycast(selectedCard.transform.position, Vector3.down, out RaycastHit areaHit))
                {
                    if (areaHit.collider.CompareTag("CardArea"))
                    {
                        // Kart� CardArea'n�n pozisyonuna sabitle ve Y pozisyonunu ayarla
                        selectedCard.transform.position = new Vector3(areaHit.collider.transform.position.x, fixedCardYPosition,
                            areaHit.collider.transform.position.z
                        );

                        isDragging = false;  // S�r�klemeyi bitir
                        selectedCard = null;  // Kart se�imini kald�r

                        LockAllCards(); // T�m kartlar�n etkile�imini kapat

                        cardAreaElements.SetActive(false);

                        // BURADA OpponentCards OBJELER�NDEN B�R�N� S�L
                        if (opponentCards != null)
                        {
                            // Kartlar� s�rayla silmek i�in kartIndex kullan�yoruz
                            string cardToDeleteName = "3DCardForGameScreen" + cardIndex;

                            // OpponentCards i�indeki belirli kart� buluyoruz
                            Transform cardToDelete = opponentCards.Find(cardToDeleteName);
                            if (cardToDelete != null)
                            {
                                Destroy(cardToDelete.gameObject); // GameObject'i sil
                                cardIndex++; // Sonraki kart� silmek i�in saya� art�r�l�r
                            }
                            else
                            {
                                Debug.Log("Silinmesi gereken kart bulunamad�.");
                            }
                        }
                        else
                        {
                            Debug.LogError("OpponentCards objesi bulunamad�.");
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

    //T�m kartlar� kilitle
    public void LockAllCards()
    {
        isLocked = true; // Kartlar� kilitle
        allCards.RemoveAll(card => card == null); // Yok edilmi� kartlar� listeden ��kar

        foreach (GameObject card in allCards)
        {
            if (card != null && card.TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = false; // Kartlar�n collider'lar�n� kapat
            }
        }
    }

    // T�m kartlar�n kilidini a�
    public void UnLockAllCards()
    {
        isLocked = false; // Kartlar�n kilidini a�
        allCards.RemoveAll(card => card == null); // Yok edilmi� kartlar� listeden ��kar

        foreach (GameObject card in allCards)
        {
            if (card != null && card.TryGetComponent<Collider>(out Collider collider))
            {
                collider.enabled = true; // Kartlar�n collider'lar�n� a�
            }
        }

        cardAreaElements.SetActive(true); // Kart alan�n� g�r�n�r yap
    }


    IEnumerator CardAreaElementFade()
    {
        yield return new WaitForSeconds(2.5f);
        cardAreaElements.SetActive(true);
    }
}