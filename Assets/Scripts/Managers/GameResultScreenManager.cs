using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameResultScreenManager : MonoBehaviour
{
    [Header("Card Prefab & Parent")]
    [SerializeField] private GameObject cardPrefab; // Kart prefab'ı
    [SerializeField] private Transform cardParent;  // Kartların yerleştirileceği parent

    [Header("Card Textures")]
    [SerializeField] private Texture2D BronzeCardTexture;
    [SerializeField] private Texture2D SilverCardTexture;
    [SerializeField] private Texture2D GoldCardTexture;

    [Header("Grid Settings")]
    [SerializeField] private float xSpacing = 2.0f; // X ekseni aralığı
    [SerializeField] private float zSpacing = 2.5f; // Z ekseni aralığı
    [SerializeField] private int columns = 4;       // Grid sütun sayısı

    FootballerInfoListWrapper footballerList;
    private List<FootballerInfo> aiProtectedCards; // Bilgisayarın koruduğu kartların bilgileri
    private List<FootballerInfo> userSelectedCards; // Bilgisayarın koruduğu kartların bilgileri

    [SerializeField] GameObject menuButton;

    private string filePath;

    void Start()
    {
        // JSON dosyasının yolunu belirle
        filePath = Path.Combine(Application.persistentDataPath, "stoleCards.json");
    }

    #region Bilgisayarın Koruduğu Kartları Göster

    public void ShowAICard()
    {
        StartCoroutine(ShowAIProtectedCardsWithAnimation());
    }

    /// <summary>
    /// Bilgisayarın koruduğu kartları JSON dosyasından okur ve grid şeklinde gösterir.
    /// </summary>
    private IEnumerator ShowAIProtectedCardsWithAnimation()
    {
        // Dosyadan JSON verisini al
        string aiProtectedCardFilePath = Path.Combine(Application.persistentDataPath, "aiProtectedCard.json");

        if (!File.Exists(aiProtectedCardFilePath))
        {
            Debug.LogError($"Bilgisayarın koruduğu kartlar JSON dosyası bulunamadı: {aiProtectedCardFilePath}");
            yield break;
        }

        // Dosyadan JSON string okuma
        string json = File.ReadAllText(aiProtectedCardFilePath);

        // JSON'u deserialize ederek listeye çevirme
        FootballerInfoListWrapper protectedFootballerList = JsonUtility.FromJson<FootballerInfoListWrapper>(json);
        if (protectedFootballerList == null || protectedFootballerList.footballers == null || protectedFootballerList.footballers.Count == 0)
        {
            Debug.LogError("JSON dosyasından geçerli korunan futbolcu bilgisi alınamadı.");
            yield break;
        }

        // Kullanıcının seçtiği kartları al
        List<FootballerInfo> userSelectedFootballers = footballerList.footballers;

        // Bilgisayarın koruduğu futbolcuları grid düzenine yerleştir
        CreateAIProtectedCardsGrid(protectedFootballerList.footballers);

        // Kartlar oluşturulurken animasyonu tetikle
        Transform protectedGridParent = cardParent.transform.Find("ProtectedGridParent");
        if (protectedGridParent == null)
        {
            Debug.LogError("ProtectedGridParent bulunamadı.");
            yield break;
        }

        #region Kartların Kontrol Edilip Arkaplanın Yeşil ya da Kırmızı Olması

        // Kartları ProtectedGridParent altında sırayla al ve animasyonu tetikle
        for (int i = 0; i < protectedFootballerList.footballers.Count; i++)
        {
            GameObject card = protectedGridParent.GetChild(i).gameObject; // ProtectedGridParent altındaki kartları al

            // Animator bileşenini al
            Animator animator = card.GetComponent<Animator>();
            if (animator != null)
            {
                animator.SetTrigger("CardProtectedAnim"); // Animasyonu tetikle
            }
            else
            {
                Debug.LogError("Kartta Animator bileşeni bulunamadı.");
            }

            // Kullanıcının seçtiği kartlarla karşılaştır
            FootballerInfo footballer = protectedFootballerList.footballers[i];
            if (userSelectedFootballers.Exists(f => f.name == footballer.name))
            {
                Debug.Log($"Kart ismi: {footballer.name} - Oyuncu Bu kartı Alamaz!"); // Kart kullanıcı tarafından seçilmiş

                // ResultGridParent objesini al
                Transform resultGridParent = GameObject.Find("ResultGridParent").transform;

                // ResultGridParent altındaki tüm objeleri kontrol et
                foreach (Transform cardObj in resultGridParent)
                {
                    // Kartın içindeki nameText objesini bul
                    Transform nameTextTransform = cardObj.Find("NameText");
                    if (nameTextTransform != null)
                    {
                        TextMeshPro nameText = nameTextTransform.GetComponent<TextMeshPro>();
                        if (nameText != null)
                        {
                            string cardName = nameText.text; // Kartın ismini al

                            // footballer listesinde isme göre eşleşen kartı bul
                            FootballerInfo matchedFootballer = protectedFootballerList.footballers
                                .FirstOrDefault(f => f.name == cardName);

                            if (matchedFootballer != null)
                            {
                                // Kart bulundu, StoleImageBG'yi kırmızı yap
                                Transform stoleImageBGTransform = cardObj.Find("Footballer/StoleImageBG");
                                if (stoleImageBGTransform != null)
                                {
                                    RawImage stoleImageBG = stoleImageBGTransform.GetComponent<RawImage>();
                                    if (stoleImageBG != null)
                                    {
                                        // Rengi kırmızıya çevir
                                        stoleImageBG.color = Color.red;
                                        Debug.Log($"Kart ismi: {cardName} - Rengi kırmızıya çevrildi.");
                                    }
                                    else
                                    {
                                        Debug.LogError($"StoleImageBG üzerinde Image bileşeni bulunamadı: {cardName}");
                                    }
                                }
                                else
                                {
                                    Debug.LogError($"Footballer/StoleImageBG bulunamadı: {cardName}");
                                }
                            }
                            else
                            {
                                Debug.Log($"Kart ismi: {cardName} - Eşleşen futbolcu bulunamadı.");
                            }
                        }
                        else
                        {
                            Debug.LogError("nameText bileşeni bulunamadı.");
                        }
                    }
                    else
                    {
                        Debug.LogError("nameText Transform bulunamadı.");
                    }
                }
            }
            else
            {
                Debug.Log($"Kart ismi: {footballer.name} - Önemsiz."); // Kart kullanıcı tarafından seçilmemiş
            }
        }

        #endregion

        #region Çalınan Kartların Kendi Oyuncuları arasına eklenmesi

        // JSON dosyasını güncelle
        string jsonFilePath = Path.Combine(Application.persistentDataPath, "myfootballers.json");

        // Korunmayan futbolcuları toplamak
        List<FootballerInfo> newFootballersToAdd = new List<FootballerInfo>();

        foreach (var footballer in userSelectedFootballers)
        {
            if (!protectedFootballerList.footballers.Exists(f => f.name == footballer.name))
            {
                newFootballersToAdd.Add(footballer);
            }
        }

        // JSON dosyasını güncelleme işlemi
        if (File.Exists(jsonFilePath))
        {
            // Mevcut JSON dosyasını oku
            string jsonData = File.ReadAllText(jsonFilePath);
            List<FootballerInfo> myFootballers = JsonUtility.FromJson<FootballerListWrapper>(jsonData).footballers;

            // Yeni futbolcuları işleme al
            foreach (var newFootballer in newFootballersToAdd)
            {
                FootballerInfo existingFootballer = myFootballers.FirstOrDefault(f => f.name == newFootballer.name);
                if (existingFootballer != null)
                {
                    // Eğer futbolcu zaten varsa, count değerini artır
                    existingFootballer.footballerCount += 1;
                    Debug.Log($"Mevcut futbolcu bulundu: {existingFootballer.name}. Count artırıldı: {existingFootballer.footballerCount}");
                }
                else
                {
                    // Eğer futbolcu yeni ekleniyorsa, count değerini 1 yap
                    newFootballer.footballerCount = 1;
                    myFootballers.Add(newFootballer);
                    Debug.Log($"Yeni futbolcu eklendi: {newFootballer.name}. Count: {newFootballer.footballerCount}");
                }
            }

            FootballerListWrapper updatedList = new FootballerListWrapper
            {
                footballers = myFootballers
            };

            File.WriteAllText(jsonFilePath, JsonUtility.ToJson(updatedList, true));
            Debug.Log("MyFootballers listesi başarıyla güncellendi.");
        }
        else
        {
            // Eğer dosya yoksa, yeni bir liste oluştur ve tüm yeni futbolcuların count değerini 1 olarak ayarla
            foreach (var newFootballer in newFootballersToAdd)
            {
                newFootballer.footballerCount = 1;
            }

            FootballerListWrapper newList = new FootballerListWrapper
            {
                footballers = newFootballersToAdd
            };

            File.WriteAllText(jsonFilePath, JsonUtility.ToJson(newList, true));
            Debug.Log("Yeni MyFootballers listesi oluşturuldu.");
        }

        #endregion

        menuButton.SetActive(true);

        #region Kartları Kontrol et ve Yeşil Yap

        // Animasyon sonrası bekleme
        yield return new WaitForSeconds(2f); // Daha uzun bir bekleme süresi

        // Burada, userSelectedFootballers listesindeki kartları kontrol et
        Transform resultGridParentForGreen = GameObject.Find("ResultGridParent").transform;

        // ResultGridParent altındaki tüm objeleri kontrol et
        foreach (Transform cardObj in resultGridParentForGreen)
        {
            // Kartın içindeki nameText objesini bul
            Transform nameTextTransform = cardObj.Find("NameText");
            if (nameTextTransform != null)
            {
                TextMeshPro nameText = nameTextTransform.GetComponent<TextMeshPro>();
                if (nameText != null)
                {
                    string cardName = nameText.text; // Kartın ismini al

                    // footballer listesinde isme göre eşleşen kartı bul
                    FootballerInfo matchedFootballer = protectedFootballerList.footballers
                        .FirstOrDefault(f => f.name == cardName);

                    // Eğer matchedFootballer bulunmazsa, yani kart protected listesinde yoksa
                    if (matchedFootballer == null && userSelectedFootballers.Exists(f => f.name == cardName))
                    {
                        // Kart bulundu, StoleImageBG'yi yeşil yap
                        Transform stoleImageBGTransform = cardObj.Find("Footballer/StoleImageBG");
                        if (stoleImageBGTransform != null)
                        {
                            RawImage stoleImageBG = stoleImageBGTransform.GetComponent<RawImage>();
                            if (stoleImageBG != null)
                            {
                                // Rengi yeşile çevir
                                stoleImageBG.color = Color.green;
                                Debug.Log($"Kart ismi: {cardName} - Rengi yeşile çevrildi.");
                            }
                            else
                            {
                                Debug.LogError($"StoleImageBG üzerinde Image bileşeni bulunamadı: {cardName}");
                            }
                        }
                        else
                        {
                            Debug.LogError($"Footballer/StoleImageBG bulunamadı: {cardName}");
                        }
                    }
                }
            }
        }

        #endregion
    }

    /// <summary>
    /// Bilgisayarın koruduğu futbolcu listesiyle grid oluşturur.
    /// </summary>
    private void CreateAIProtectedCardsGrid(List<FootballerInfo> footballers)
    {
        // Grid için bir parent oluştur
        GameObject parent = new GameObject("ProtectedGridParent");
        parent.transform.SetParent(cardParent);

        // Parent'ın pozisyonunu sıfırla
        parent.transform.localPosition = Vector3.zero;

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = cardPrefab.transform.localPosition;
        Quaternion prefabRotation = cardPrefab.transform.localRotation;

        // Grid düzeni oluştur
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;    // Satır hesaplama
            int column = i % columns; // Sütun hesaplama

            // Kartın pozisyonunu hesapla
            Vector3 localPosition = new Vector3(column * xSpacing + 0.15f, 0, row * zSpacing + 0.05f);
            Vector3 position = prefabPosition + localPosition;

            // Kartı oluştur ve parent'a ekle
            GameObject card = Instantiate(cardPrefab, position, prefabRotation, parent.transform);

            // Kart detaylarını doldur
            ShowCardDetailsAICards(card, footballers[i]);
        }
    }

    /// <summary>
    /// Verilen futbolcu bilgisiyle bir kart oluşturur ve UI'ye ekler.
    /// </summary>
    private void ShowCardDetailsAICards(GameObject card, FootballerInfo footballer)
    {
        // Kartın Renderer'ına erişim
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileşeni bulunamadı.");
            return;
        }

        // Kartın texturesini belirleme
        Texture2D selectedTexture = null;
        switch (footballer.packageType)
        {
            case "Bronze":
                selectedTexture = BronzeCardTexture;
                break;
            case "Silver":
                selectedTexture = SilverCardTexture;
                break;
            case "Gold":
                selectedTexture = GoldCardTexture;
                break;
            default:
                Debug.LogError("Bilinmeyen kart türü!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // Kart üzerindeki metin alanlarını güncelleme
        Transform nameTextTransform = card.transform.Find("NameText");
        if (nameTextTransform != null)
        {
            TextMeshPro nameText = nameTextTransform.GetComponent<TextMeshPro>();
            if (nameText != null) nameText.text = footballer.name;
        }

        Transform ratingTextTransform = card.transform.Find("RatingText");
        if (ratingTextTransform != null)
        {
            TextMeshPro ratingText = ratingTextTransform.GetComponent<TextMeshPro>();
            if (ratingText != null) ratingText.text = footballer.rating;
        }

        Transform priceTextTransform = card.transform.Find("PriceText");
        if (priceTextTransform != null)
        {
            TextMeshPro priceText = priceTextTransform.GetComponent<TextMeshPro>();
            if (priceText != null) priceText.text = FormatPrice(int.Parse(footballer.price));
        }

        // Görselleri yükleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            // Görselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
    }

    #endregion


    #region Oyucunun Çalmak istediği kartları Göster


    /// <summary>
    /// JSON dosyasından futbolcuları okur ve grid şeklinde gösterir.
    /// </summary>
    private void ShowStolenCards()
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON dosyası bulunamadı: {filePath}");
            return;
        }

        // Dosyadan JSON string okuma
        string json = File.ReadAllText(filePath);

        // JSON'u deserialize ederek listeye çevirme
        footballerList = JsonUtility.FromJson<FootballerInfoListWrapper>(json);
        if (footballerList == null || footballerList.footballers == null || footballerList.footballers.Count == 0)
        {
            Debug.LogError("JSON dosyasından geçerli futbolcu bilgisi alınamadı.");
            return;
        }

        // Grid oluşturma
        CreateStolenCardsGrid(footballerList.footballers);
    }

    /// <summary>
    /// Verilen futbolcu listesiyle grid oluşturur.
    /// </summary>
    private void CreateStolenCardsGrid(List<FootballerInfo> footballers)
    {
        // Grid için bir parent oluştur
        GameObject parent = new GameObject("ResultGridParent");
        parent.transform.SetParent(cardParent);

        // Parent'ın pozisyonunu sıfırla
        parent.transform.localPosition = Vector3.zero;

        // Prefab'in pozisyonunu ve rotasyonunu al
        Vector3 prefabPosition = cardPrefab.transform.localPosition;
        Quaternion prefabRotation = cardPrefab.transform.localRotation;

        // Grid düzeni oluştur
        for (int i = 0; i < footballers.Count; i++)
        {
            int row = i / columns;    // Satır hesaplama
            int column = i % columns; // Sütun hesaplama

            // Kartın pozisyonunu hesapla
            Vector3 localPosition = new Vector3(column * xSpacing + 0.15f, 0, row * zSpacing - 0.165f);
            Vector3 position = prefabPosition + localPosition;

            // Kartı oluştur ve parent'a ekle
            GameObject card = Instantiate(cardPrefab, position, prefabRotation, parent.transform);

            // Kart detaylarını doldur
            ShowCardDetails(card, footballers[i]);
        }
    }

    /// <summary>
    /// Verilen futbolcu bilgisiyle bir kart oluşturur ve UI'ye ekler.
    /// </summary>
    private void ShowCardDetails(GameObject card, FootballerInfo footballer)
    {
        // Kartın Renderer'ına erişim
        Renderer cardRenderer = card.GetComponent<Renderer>();
        if (cardRenderer == null)
        {
            Debug.LogError("Kart üzerinde Renderer bileşeni bulunamadı.");
            return;
        }

        // Kartın texturesini belirleme
        Texture2D selectedTexture = null;
        switch (footballer.packageType)
        {
            case "Bronze":
                selectedTexture = BronzeCardTexture;
                break;
            case "Silver":
                selectedTexture = SilverCardTexture;
                break;
            case "Gold":
                selectedTexture = GoldCardTexture;
                break;
            default:
                Debug.LogError("Bilinmeyen kart türü!");
                return;
        }

        // Texture'u karta uygulama
        foreach (Material mat in cardRenderer.materials)
        {
            mat.mainTexture = selectedTexture;
        }

        // Kart üzerindeki metin alanlarını güncelleme
        Transform nameTextTransform = card.transform.Find("NameText");
        if (nameTextTransform != null)
        {
            TextMeshPro nameText = nameTextTransform.GetComponent<TextMeshPro>();
            if (nameText != null) nameText.text = footballer.name;
        }

        Transform ratingTextTransform = card.transform.Find("RatingText");
        if (ratingTextTransform != null)
        {
            TextMeshPro ratingText = ratingTextTransform.GetComponent<TextMeshPro>();
            if (ratingText != null) ratingText.text = footballer.rating;
        }

        Transform priceTextTransform = card.transform.Find("PriceText");
        if (priceTextTransform != null)
        {
            TextMeshPro priceText = priceTextTransform.GetComponent<TextMeshPro>();
            if (priceText != null) priceText.text = FormatPrice(int.Parse(footballer.price));
        }

        // Görselleri yükleme
        Transform footballerObject = card.transform.Find("Footballer");
        if (footballerObject != null)
        {
            RawImage playerImage = footballerObject.Find("FootballerImage").GetComponent<RawImage>();
            RawImage countryFlagImage = footballerObject.Find("CountryFlag").GetComponent<RawImage>();
            RawImage teamLogoImage = footballerObject.Find("TeamLogo").GetComponent<RawImage>();

            GameObject StoleImagebg = footballerObject.Find("StoleImageBG").gameObject;
            GameObject StoleImageHand = footballerObject.Find("StoleHandImage").gameObject;

            StoleImagebg.SetActive(true);
            StoleImageHand.SetActive(true);

            // Görselleri FootballerInfo'dan alarak kartlara ekleme
            playerImage.texture = TextureCache.Instance.LoadTexture("MyRepository/FootballerPhotos", footballer.playerImageName);
            countryFlagImage.texture = TextureCache.Instance.LoadTexture("MyRepository/CountryPhotos", footballer.countryFlagImageName);
            teamLogoImage.texture = TextureCache.Instance.LoadTexture("MyRepository/TeamPhotos", footballer.teamLogoImageName);
        }
    }

    #endregion

    /// <summary>
    /// Fiyatları daha okunabilir bir biçimde biçimlendirir (örn. "1000000" → "1,000,000").
    /// </summary>
    private string FormatPrice(int price)
    {
        return price.ToString("N0"); // Virgülle ayırarak biçimlendirme
    }
}

[System.Serializable]
public class FootballerListWrapper
{
    public List<FootballerInfo> footballers;
}

