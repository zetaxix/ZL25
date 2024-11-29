using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LeaderboardManager : MonoBehaviour
{
    [SerializeField] private GameObject leaderboardEntryPrefab; // Her bir sat�r i�in prefab
    [SerializeField] private Transform leaderboardContent;      // Scroll View i�eri�i
    [SerializeField] private PHPManager phpManager;            // PHPManager referans�
    private List<LeaderboardEntry> leaderboardEntries = new List<LeaderboardEntry>(); // Liste verisi

    private void Start()
    {
        // Kullan�c� verileri haz�r oldu�unda liderlik tablosunu g�ncelle
        StartCoroutine(WaitAndLoadLeaderboard());
    }

    private IEnumerator WaitAndLoadLeaderboard()
    {
        // PHPManager verileri �ekene kadar bekle
        while (phpManager.users.Count == 0)
        {
            yield return null;
        }

        // Kullan�c� verilerini i�leme ve liderlik tablosunu olu�turma
        GenerateLeaderboardFromUsers();
        UpdateRanks();
        UpdateLeaderboard();
    }

    private void GenerateLeaderboardFromUsers()
    {
        // PHPManager'dan kullan�c�lar� al ve skorlar� rastgele ata
        foreach (var user in phpManager.users)
        {
            leaderboardEntries.Add(new LeaderboardEntry
            {
                Username = user.username,
                Score = Random.Range(1000, 2000) // Rastgele skor
            });
        }

        // Kendi kullan�c� ad�n� tabloya ekle
        AddUserToLeaderboard();
    }

    private void AddUserToLeaderboard()
    {
        // Kullan�c� ad�n� PlayerPrefs'ten al
        string username = PlayerPrefs.GetString("username", "MyPlayer");

        // Rastgele bir pozisyona eklemek i�in rastgele bir index se�
        int randomIndex = Random.Range(0, leaderboardEntries.Count);

        // Kendi skorunuzu belirleyin
        int userScore = Random.Range(1000, 2000);

        // Kendi kullan�c� giri�ini olu�tur
        LeaderboardEntry userEntry = new LeaderboardEntry { Username = username, Score = userScore };

        // Rastgele pozisyona ekle
        leaderboardEntries.Insert(randomIndex, userEntry);
    }

    private void UpdateRanks()
    {
        // S�ralamay� g�ncelle
        leaderboardEntries.Sort((a, b) => b.Score.CompareTo(a.Score));
        for (int i = 0; i < leaderboardEntries.Count; i++)
        {
            leaderboardEntries[i].Rank = i + 1;
        }
    }

    public void UpdateLeaderboard()
    {
        // �nceden eklenmi� giri�leri temizle
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }

        // Her bir giri� i�in prefab olu�tur
        foreach (var entry in leaderboardEntries)
        {
            GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContent);

            // Prefab �zerindeki TextMeshPro bile�enlerini bul ve de�er ata
            TextMeshProUGUI rankText = entryObject.transform.Find("rank").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI usernameText = entryObject.transform.Find("username").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI scoreText = entryObject.transform.Find("score").GetComponent<TextMeshProUGUI>();

            rankText.text = entry.Rank.ToString();
            usernameText.text = entry.Username;
            scoreText.text = entry.Score.ToString();

            // E�er giri� kendi kullan�c� ad�n� i�eriyorsa rengi de�i�tir
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
