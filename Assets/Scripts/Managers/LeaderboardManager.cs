using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardEntryPrefab; // Her bir satýr için prefab
    [SerializeField] private Transform leaderboardContent;      // Scroll View içeriði
    [SerializeField] private PHPManager phpManager;            // PHPManager referansý
    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>(); // Liste verisi

    private void Start()
    {
        // Kullanýcý verileri hazýr olduðunda liderlik tablosunu güncelle
        StartCoroutine(WaitAndLoadLeaderboard());
    }

    private IEnumerator WaitAndLoadLeaderboard()
    {
        // PHPManager verileri çekene kadar bekle
        while (phpManager.users.Count == 0)
        {
            yield return null;
        }

        // Kullanýcý verilerini iþleme ve liderlik tablosunu oluþturma
        GenerateLeaderboardFromUsers();
        UpdateRanks();
        UpdateLeaderboard();
    }

    private void GenerateLeaderboardFromUsers()
    {
        // PHPManager'dan kullanýcýlarý al ve skorlarý rastgele ata
        foreach (var user in phpManager.users)
        {
            leaderboardEntries.Add(new LeaderboardEntry
            {
                Username = user.username,
                Score = Random.Range(1000, 2000) // Rastgele skor
            });
        }

        // Kendi kullanýcý adýný tabloya ekle
        AddUserToLeaderboard();
    }

    private void AddUserToLeaderboard()
    {
        // Kullanýcý adýný PlayerPrefs'ten al
        string username = PlayerPrefs.GetString("username", "MyPlayer");

        // Rastgele bir pozisyona eklemek için rastgele bir index seç
        int randomIndex = Random.Range(0, leaderboardEntries.Count);

        // Kendi skorunuzu belirleyin
        int userScore = Random.Range(1000, 2000);

        // Kendi kullanýcý giriþini oluþtur
        LeaderboardEntry userEntry = new LeaderboardEntry { Username = username, Score = userScore };

        // Rastgele pozisyona ekle
        leaderboardEntries.Insert(randomIndex, userEntry);
    }

    private void UpdateRanks()
    {
        // Sýralamayý güncelle
        leaderboardEntries.Sort((a, b) => b.Score.CompareTo(a.Score));
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            leaderboardEntries[i].Rank = i + 1;
        }
    }

    public void UpdateLeaderboard()
    {
        // Önceden eklenmiþ giriþleri temizle
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // Her bir giriþ için prefab oluþtur
        foreach (var entry in leaderboardEntries)
        {
            GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContent);

            // Prefab üzerindeki TextMeshPro bileþenlerini bul ve deðer ata
            TextMeshProUGUI rankText = entryObject.transform.Find("rank").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI usernameText = entryObject.transform.Find("username").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryObject.transform.Find("score").GetComponent<TextMeshProUGUI>();

            rankText.text = entry.Rank.ToString();
            usernameText.text = entry.Username;
            scoreText.text = entry.Score.ToString();

            // Eðer giriþ kendi kullanýcý adýný içeriyorsa rengi deðiþtir
            if (entry.Username == PlayerPrefs.GetString("username", "MyPlayer"))
            {
                rankText.color = Color.yellow;
                usernameText.color = Color.yellow;
                scoreText.color = Color.yellow;
            }
        }
    }
}

[System.Serializable]
public class LeaderboardEntry
{
    public int Rank;
    public string Username;
    public int Score;
}
