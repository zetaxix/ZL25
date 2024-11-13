using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardManager : MonoBehaviour
{
    // Paketler i�in futbolcu listeleri
    public List<Footballer> bronzePackage = new List<Footballer>();
    public List<Footballer> silverPackage = new List<Footballer>();
    public List<Footballer> goldPackage = new List<Footballer>();

    [Header("Footballer Card Settings")]
    // Kart Prefab'�
    [SerializeField] GameObject footballerCardPrefab;

    [Header("Card Pics")]
    [SerializeField] Texture2D BronzeCardPicture;
    [SerializeField] Texture2D SilverCardPicture;
    [SerializeField] Texture2D GoldCardPicture;

    [Header("ParticleSystems")]
    [SerializeField] ParticleSystem bronzeFireworkSystem;
    [SerializeField] ParticleSystem silverFireworkSystem;
    [SerializeField] ParticleSystem goldFireworkSystem;

    // Kart�n sahnedeki referans�
    private GameObject card;

    // Paket t�r�n� alacak bir fonksiyon
    public void SetPackageType(string packageType)
    {
        Debug.Log($"A��lan paket t�r�: {packageType}");

        // Paket t�r�ne g�re kart a�ma i�lemi yap�labilir
        ShowFootballerCard(packageType);
    }

    // Futbolcu kart�n� g�sterme fonksiyonu
    void ShowFootballerCard(string packageType)
    {
        // Paket t�r�ne g�re futbolcular� se�
        List<Footballer> selectedPackage = new List<Footballer>();

        if (packageType == "Bronze")
        {
            selectedPackage = bronzePackage;
        }
        else if (packageType == "Silver")
        {
            selectedPackage = silverPackage;
        }
        else if (packageType == "Gold")
        {
            selectedPackage = goldPackage;
        }

        // E�er paket bo� de�ilse, rastgele bir futbolcu se�
        if (selectedPackage.Count > 0)
        {
            int randomIndex = Random.Range(0, selectedPackage.Count);
            Footballer selectedFootballer = selectedPackage[randomIndex];

            // Kart� olu�tur
            CreateFootballerCard(selectedFootballer, packageType);
        }
        else
        {
            Debug.LogWarning("Se�ilen paket bo�.");
        }
    }

    // Futbolcu kart� olu�turma
    void CreateFootballerCard(Footballer footballer, string cardType)
    {
        card = Instantiate(footballerCardPrefab, footballerCardPrefab.transform.position, footballerCardPrefab.transform.rotation);

        // Card'daki renderer bile�enine ula��n
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart �zerinde Renderer bile�eni bulunamad�.");
            return;
        }

        // De�i�tirilecek texture'� belirleyin
        Texture2D selectedTexture = null;
        if (cardType == "Bronze")
        {
            selectedTexture = BronzeCardPicture;
        }
        else if (cardType == "Silver")
        {
            selectedTexture = SilverCardPicture;
        }
        else if (cardType == "Gold")
        {
            selectedTexture = GoldCardPicture;
        }

        // E�er bir texture se�ildiyse, t�m materyallerin ana texture'�n� g�ncelleyin
        if (selectedTexture != null)
        {
            foreach (Material mat in cardRenderer.materials)
            {
                mat.mainTexture = selectedTexture;
            }
        }

        // Kart�n i�indeki UI ��elerini al
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Footballer GameObject'inin i�indeki RawImage'lar� bulmak i�in tam yolu belirtin
        Transform footballerObject = card.transform.Find("Footballer");

        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // UI ��elerine futbolcu bilgilerini aktar
            nameText.text = footballer.name;
            ratingText.text = footballer.rating.ToString();
            priceText.text = FormatPrice(footballer.price);

            // Texture2D ��elerini RawImage'lara aktar
            playerImage.texture = footballer.footballerImage;
            countryFlagImage.texture = footballer.countryFlag;
            teamLogoImage.texture = footballer.teamLogo;
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamad�.");
        }

        StartCoroutine(FireworkEffect(cardType));

        Debug.Log("Futbolcu Kart� Olu�turuldu: " + footballer.name);
    }

    string FormatPrice(int price)
    {
        if (price >= 1000000)
        {
            // Milyon format� i�in
            return (price / 1000000f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "M�";
        }
        else if (price >= 1000)
        {
            // Binlik format i�in
            return (price / 1000f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "K�";
        }
        else
        {
            // Bin alt�ndaki fiyatlar i�in
            return price.ToString() + "�";
        }
    }

    IEnumerator FireworkEffect(string cardType)
    {
        yield return new WaitForSeconds(4);
        if (cardType == "Bronze")
        {
            bronzeFireworkSystem.Play();
        }
        else if (cardType == "Silver")
        {
            silverFireworkSystem.Play();
        }
        else if (cardType == "Gold")
        {
            goldFireworkSystem.Play();
        }
        else
        {
            Debug.Log("Ge�ersiz Kart T�r�!");
        }
    }

    public void CloseThePackageOpenScreen()
    {
        bronzeFireworkSystem.Stop();
        silverFireworkSystem.Stop();
        goldFireworkSystem.Stop();
    }

    public void DestroyCard()
    {
        if (card != null)
        {
            Destroy(card);
            card = null; // Referans� s�f�rlay�n
        }
    }

}
