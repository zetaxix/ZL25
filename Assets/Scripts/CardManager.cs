using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class CardManager : MonoBehaviour
{
    // Paketler için futbolcu listeleri
    public List<Footballer> bronzePackage = new List<Footballer>();
    public List<Footballer> silverPackage = new List<Footballer>();
    public List<Footballer> goldPackage = new List<Footballer>();

    [Header("Footballer Card Settings")]
    // Kart Prefab'ý
    [SerializeField] GameObject footballerCardPrefab;

    [Header("Card Pics")]
    [SerializeField] Texture2D BronzeCardPicture;
    [SerializeField] Texture2D SilverCardPicture;
    [SerializeField] Texture2D GoldCardPicture;

    [Header("ParticleSystems")]
    [SerializeField] ParticleSystem bronzeFireworkSystem;
    [SerializeField] ParticleSystem silverFireworkSystem;
    [SerializeField] ParticleSystem goldFireworkSystem;

    // Kartýn sahnedeki referansý
    private GameObject card;

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
            CreateFootballerCard(selectedFootballer, packageType);
        }
        else
        {
            Debug.LogWarning("Seçilen paket boþ.");
        }
    }

    // Futbolcu kartý oluþturma
    void CreateFootballerCard(Footballer footballer, string cardType)
    {
        card = Instantiate(footballerCardPrefab, footballerCardPrefab.transform.position, footballerCardPrefab.transform.rotation);

        // Card'daki renderer bileþenine ulaþýn
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileþeni bulunamadý.");
            return;
        }

        // Deðiþtirilecek texture'ý belirleyin
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

        // Eðer bir texture seçildiyse, tüm materyallerin ana texture'ýný güncelleyin
        if (selectedTexture != null)
        {
            foreach (Material mat in cardRenderer.materials)
            {
                mat.mainTexture = selectedTexture;
            }
        }

        // Kartýn içindeki UI öðelerini al
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        // Footballer GameObject'inin içindeki RawImage'larý bulmak için tam yolu belirtin
        Transform footballerObject = card.transform.Find("Footballer");

        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // UI öðelerine futbolcu bilgilerini aktar
            nameText.text = footballer.name;
            ratingText.text = footballer.rating.ToString();
            priceText.text = FormatPrice(footballer.price);

            // Texture2D öðelerini RawImage'lara aktar
            playerImage.texture = footballer.footballerImage;
            countryFlagImage.texture = footballer.countryFlag;
            teamLogoImage.texture = footballer.teamLogo;
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }

        SaveFootballersInfo(cardType);

        FireworkEffect(cardType);

        Debug.Log("Futbolcu Kartý Oluþturuldu: " + footballer.name);
    }

    void SaveFootballersInfo(string packageType)
    {
        // Kartýn içindeki UI öðelerini al
        TextMeshPro nameText = card.transform.Find("NameText").GetComponent<TextMeshPro>();
        TextMeshPro ratingText = card.transform.Find("RatingText").GetComponent<TextMeshPro>();
        TextMeshPro priceText = card.transform.Find("PriceText").GetComponent<TextMeshPro>();

        Transform footballerObject = card.transform.Find("Footballer");

        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // Futbolcu bilgilerini bir FootballerInfo nesnesine kaydet
            FootballerInfo newFootballerInfo = new FootballerInfo
            {
                name = nameText.text,
                rating = ratingText.text,
                price = priceText.text,
                playerImageName = playerImage.texture.name,
                countryFlagImageName = countryFlagImage.texture.name,
                teamLogoImageName = teamLogoImage.texture.name,
                packageType = packageType, // Paket türünü kaydediyoruz
                footballerCount = 1 // Yeni futbolcu için baþlangýç deðeri
            };

            string filePath = Path.Combine(Application.persistentDataPath, "myfootballers.json");

            // Mevcut futbolcularýn listesini yükle
            List<FootballerInfo> footballersList = new List<FootballerInfo>();
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                footballersList = JsonUtility.FromJson<FootballerInfoList>(existingJson)?.footballers ?? new List<FootballerInfo>();
            }

            // Ayný futbolcu var mý kontrol et
            FootballerInfo existingFootballer = footballersList.Find(f => f.name == newFootballerInfo.name);

            if (existingFootballer != null)
            {
                // Eðer ayný futbolcu varsa, count deðerini artýr
                existingFootballer.footballerCount++;
            }
            else
            {
                // Eðer yeni bir futbolcuysa listeye ekle
                footballersList.Add(newFootballerInfo);
            }

            // Güncellenmiþ listeyi tekrar JSON formatýna çevirip dosyaya kaydet
            FootballerInfoList footballerInfoList = new FootballerInfoList { footballers = footballersList };
            string updatedJson = JsonUtility.ToJson(footballerInfoList, true);
            File.WriteAllText(filePath, updatedJson);

            Debug.Log($"Futbolcu bilgileri kaydedildi: {newFootballerInfo.name} - Paket Türü: {newFootballerInfo.packageType}");
        }
        else
        {
            Debug.LogError("Footballer GameObject'i bulunamadý.");
        }
    }

    string FormatPrice(int price)
    {
        if (price >= 1000000)
        {
            // Milyon formatý için
            return (price / 1000000f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "M€";
        }
        else if (price >= 1000)
        {
            // Binlik format için
            return (price / 1000f).ToString("0.#", System.Globalization.CultureInfo.InvariantCulture) + "K€";
        }
        else
        {
            // Bin altýndaki fiyatlar için
            return price.ToString() + "€";
        }
    }

    public void FireworkEffect(string cardType)
    {
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
            Debug.Log("Geçersiz Kart Türü!");
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

            PlayerPrefs.DeleteKey("OpenAnimSkip");

            card = null; // Referansý sýfýrlayýn
        }
    }

}

[System.Serializable]
public class FootballerInfo
{
    public string name;
    public string rating;
    public string price;
    public string playerImageName;
    public string countryFlagImageName;
    public string teamLogoImageName;
    public int footballerCount;
    public string packageType; // Yeni eklenen alan
}

// Futbolcu listesini JSON formatýnda tutmak için sýnýf
[System.Serializable]
public class FootballerInfoList
{
    public List<FootballerInfo> footballers;
}
