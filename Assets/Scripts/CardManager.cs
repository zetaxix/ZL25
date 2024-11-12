using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardManager : MonoBehaviour
{
    // Paketler için futbolcu listeleri
    public List<Footballer> bronzePackage = new List<Footballer>();
    public List<Footballer> silverPackage = new List<Footballer>();
    public List<Footballer> goldPackage = new List<Footballer>();

    // Kart Prefab'ý
    [SerializeField] GameObject footballerCardPrefab;
    [SerializeField] Transform cardParent;  // Kartlarýn gösterileceði alan

    [SerializeField] ParticleSystem fireWorkSystem;

    // Paket türünü alacak bir fonksiyon
    public void SetPackageType(string packageType)
    {
        Debug.Log($"Açýlan paket türü: {packageType}");

        // Paket türüne göre kart açma iþlemi yapýlabilir
        ShowFootballerCard(packageType);
    }

    // Futbolcu kartýný gösterme fonksiyonu
    void ShowFootballerCard(string packageType)
    {
        // Paket türüne göre futbolcularý seç
        List<Footballer> selectedPackage = new List<Footballer>();

        if (packageType == "Bronze")
        {
            selectedPackage = bronzePackage;
            Debug.Log("Bronz Seçildi!");
        }
        else if (packageType == "Silver")
        {
            selectedPackage = silverPackage;
        }
        else if (packageType == "Gold")
        {
            selectedPackage = goldPackage;
        }

        // Eðer paket boþ deðilse, rastgele bir futbolcu seç
        if (selectedPackage.Count > 0)
        {
            int randomIndex = Random.Range(0, selectedPackage.Count);
            Footballer selectedFootballer = selectedPackage[randomIndex];

            // Kartý oluþtur
            CreateFootballerCard(selectedFootballer);
        }
        else
        {
            Debug.LogWarning("Seçilen paket boþ.");
        }
    }

    // Futbolcu kartý oluþturma
    void CreateFootballerCard(Footballer footballer)
    {
        GameObject card = Instantiate(footballerCardPrefab, cardParent);

        // Kartýn içindeki UI öðelerini al
        TextMeshProUGUI nameText = card.transform.Find("NameText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI ratingText = card.transform.Find("RatingText").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI priceText = card.transform.Find("PriceText").GetComponent<TextMeshProUGUI>();

        RawImage playerImage = card.transform.Find("FootballerImage").GetComponent<RawImage>();
        RawImage countryFlagImage = card.transform.Find("CountryFlag").GetComponent<RawImage>();
        RawImage teamLogoImage = card.transform.Find("TeamLogo").GetComponent<RawImage>();

        // UI öðelerine futbolcu bilgilerini aktar
        nameText.text = footballer.name;
        ratingText.text = footballer.rating.ToString();
        priceText.text = FormatPrice(footballer.price);

        // RawImage bileþenlerine Texture2D türündeki görselleri ata
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

        Debug.Log("Futbolcu Kartý Oluþturuldu: " + footballer.name);
    }

    string FormatPrice(int price)
    {
        if (price >= 1000000)
        {
            return (price / 1000000f).ToString("0.#") + "M€";
        }
        else if (price >= 1000)
        {
            return (price / 1000f).ToString("0.#") + "K€";
        }
        else
        {
            return price.ToString() + "€";
        }
    }

    IEnumerator FireworkEffect()
    {
        yield return new WaitForSeconds(3);
        fireWorkSystem.gameObject.SetActive(true);
    }

}
