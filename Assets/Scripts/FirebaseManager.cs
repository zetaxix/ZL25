using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    FirebaseAuth firebaseAuth;
    public FirebaseUser user;

    public Toggle rememberMeToggle;

    private void Awake()
    {
        findToggle();

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

    private void Update()
    {
        findToggle();
    }

    public void findToggle()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            rememberMeToggle = GameObject.Find("RememberToggle").GetComponent<Toggle>();
        }

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
            user = newUser;
           
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
            user = result.User;

            FirebaseUIManager.Instance.isAccountPass = true;
            FirebaseUIManager.Instance.isRememberMe = true;
            
            Debug.Log("<color=green> User signed in successfully: </color>" + user.DisplayName + " User Id: " + user.UserId);

        });
    }

    public void SignOut()
    {
        if (firebaseAuth != null)
        {
            firebaseAuth.SignOut();
            Debug.Log("User signed out successfully.");
        }
        else
        {
            Debug.LogError("FirebaseAuth instance is null! Sign out failed.");
        }
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
