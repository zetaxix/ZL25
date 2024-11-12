using Firebase;
using Firebase.Analytics;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseAnalyticsMethod : MonoBehaviour
{
    public static FirebaseAnalyticsMethod instance;

    private void Awake()
    {
        // Singleton yap�s�n� olu�tur
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); // Birden fazla instance olmas�n� engelle
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;

            FirebaseAnalytics.LogEvent("game_start"); // �lk a��l��ta olay kayd�.
        });
    }
}
