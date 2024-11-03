using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    FirebaseAuth firebaseAuth;

    private void Awake()
    {
        // Singleton yapýsýný oluþtur
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // FirebaseManager sahneler arasý geçiþte kaybolmaz
        }
        else
        {
            Destroy(gameObject); // Birden fazla instance olmasýný engelle
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => 
        {
            FirebaseApp app = FirebaseApp.DefaultInstance;
            firebaseAuth = FirebaseAuth.DefaultInstance;
        });
    }

    public void Register(string email, string password)
    {
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Register was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Register encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser newUser = result.User;
            SceneManager.LoadScene(1);
            Debug.LogFormat("User registered successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
        });
    }

    public void Login(string email, string password)
    {
        firebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Login was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Login encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser user = result.User;

            Debug.Log("<color=green> User signed in successfully: </color>" + user.DisplayName + " User Id: " + user.UserId);

            SceneManager.LoadScene(1);
        });
    }

    public void ResetPassword(string email)
    {
        firebaseAuth.SendPasswordResetEmailAsync(email).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Password reset was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Password reset encountered an error: " + task.Exception);
                return;
            }

            Debug.Log("Password reset email sent successfully.");
        });
    }

}
