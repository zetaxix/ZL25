using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FirebaseUIManager : MonoBehaviour
{
    public Button m_loginButton;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;

    private void Start()
    {
        m_loginButton.onClick.AddListener(LoginAccount);
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

        Debug.Log(emailText);

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
