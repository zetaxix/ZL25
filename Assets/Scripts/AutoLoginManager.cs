using UnityEngine;

public class AutoLoginManager : MonoBehaviour
{
    private void Start()
    {
        // Giriþ kontrolünü 0.5 saniye gecikmeli olarak yapýyoruz
        Invoke("AttemptAutoLogin", 0.2f);
    }

    private void AttemptAutoLogin()
    {
        if (PlayerPrefs.GetInt("RememberMe", 0) == 1)
        {
            string savedEmail = PlayerPrefs.GetString("SavedEmail");
            string savedPassword = PlayerPrefs.GetString("SavedPassword");

            if (FirebaseUIManager.Instance != null)
            {
                FirebaseUIManager.Instance.emailInput.text = savedEmail;
                FirebaseUIManager.Instance.passwordInput.text = savedPassword;
                FirebaseUIManager.Instance.LoginAccount();
            }
            else
            {
                Debug.LogError("FirebaseUIManager instance is null!");
            }
        }
    }
}