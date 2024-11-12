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

    // Kart Prefab'�
    [SerializeField] GameObject footballerCardPrefab;
    [SerializeField] Transform cardParent;  // Kartlar�n g�sterilece�i alan

    [SerializeField] ParticleSystem fireWorkSystem;

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
            Debug.Log("Bronz Se�ildi!");
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
            CreateFootballerCard(selectedFootballer);
        }
        else
        {
            Debug.LogWarning("Se�ilen paket bo�.");
        }
    }

    // Futbolcu kart� olu�turma
    void CreateFootballerCard(Footballer footballer)
    {
        GameObject card = Instantiate(footballerCardPrefab, cardParent);

        // Kart�n i�indeki UI ��elerini al
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        // UI ��elerine futbolcu bilgilerini aktar
        nameText.text = footballer.name;
        ratingText.text = footballer.rating.ToString();
        priceText.text = FormatPrice(footballer.price);

        // RawImage bile�enlerine Texture2D t�r�ndeki g�rselleri ata
        if (footballer.footballerImage != null)
        {
            playerImage.texture = footballer.footballerImage;
        }

        if (footballer.countryFlag != null)
        {
            countryFlagImage.texture = footballer.countryFlag;
        }

        if (footballer.teamLogo != null)
        {
            teamLogoImage.texture = footballer.teamLogo;
        }

        StartCoroutine(FireworkEffect());

        Debug.Log("Futbolcu Kart� Olu�turuldu: " + footballer.name);
    }

    string FormatPrice(int price)
    {
        if (price >= 1000000)
        {
            return (price / 1000000f).ToString("0.#") + "M�";
        }
        else if (price >= 1000)
        {
            return (price / 1000f).ToString("0.#") + "K�";
        }
        else
        {
            return price.ToString() + "�";
        }
    }

    IEnumerator FireworkEffect()
    {
        yield return new WaitForSeconds(3);
        fireWorkSystem.gameObject.SetActive(true);
    }

}
