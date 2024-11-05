using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;

public class FirebaseUIManager : MonoBehaviour
{
    public static FirebaseUIManager Instance { get; private set; }
    
    public Button m_loginButton;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    public bool isAccountPass = false;
    public bool isRememberMe = false;
    private void Start()
    {
        m_loginButton.onClick.AddListener(LoginAccount);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isAccountPass)
        {
            SceneManager.LoadScene("MenuScene");
            isAccountPass = false;
            Destroy(gameObject);
        }

        if (isRememberMe)
        {
            // Login baþarýlý olduktan sonra "Beni Hatýrla" seçeneðini kaydediyoruz
            if (FirebaseManager.instance.rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt("RememberMe", 1); // 1 = true
                PlayerPrefs.SetString("SavedEmail", FirebaseUIManager.Instance.emailInput.text);
                PlayerPrefs.SetString("SavedPassword", FirebaseUIManager.Instance.passwordInput.text);
            }
            else
            {
                PlayerPrefs.DeleteKey("RememberMe"); // RememberMe kapalý ise kayýtlarý sil
                PlayerPrefs.DeleteKey("SavedEmail");
                PlayerPrefs.DeleteKey("SavedPassword");
            }
            isRememberMe=false;
            PlayerPrefs.Save();
        }
    }

    public void RegisterAccount()
    {
        string emailText = emailInput.text;
        string passwordText = passwordInput.text;

        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.Register(emailText, passwordText);
        }
        else
        {
            Debug.LogError("FirebaseManager instance is null!");
        }
    }

    public void LoginAccount()
    {
        string emailText = emailInput.text;
        string passwordText = passwordInput.text;

        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.Login(emailText, passwordText);
        }
        else
        {
            Debug.LogError("FirebaseManager instance is null!");
        }
    }

    public void ResetPasswordAccount()
    {
        string emailText = emailInput.text;

        if (FirebaseManager.instance != null)
        {
            FirebaseManager.instance.ResetPassword(emailText);
        }
        else
        {
            Debug.LogError("FirebaseManager instance is null!");
        }
    }
}
