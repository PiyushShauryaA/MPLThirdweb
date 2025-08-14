using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour
{
    public static LoginManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;

    private const string LOGIN_URL = "http://localhost:3000/login";

    public void OnLoginButtonClicked()
    {
        Debug.Log("Login button clicked!"); // Log when button is pressed

        if (usernameInput == null || passwordInput == null || loginButton == null)
        {
            Debug.LogError("LoginManager: One or more UI references are not set!");
            return;
        }

        string username = usernameInput.text;
        string password = passwordInput.text;

        Debug.Log($"Username entered: {username}");
        Debug.Log($"Password entered: {password}");

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("Username and password cannot be empty!");
            return;
        }

        StartCoroutine(SendLoginRequest(username, password));
    }

    private IEnumerator SendLoginRequest(string username, string password)
    {
        Debug.Log("Preparing to send login request...");

        // Create JSON payload
        LoginData loginData = new LoginData
        {
            username = username,
            password = password
        };

        string jsonData = JsonUtility.ToJson(loginData);
        Debug.Log("JSON to send: " + jsonData);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        // Create UnityWebRequest
        using (UnityWebRequest request = new UnityWebRequest(LOGIN_URL, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            // Disable button during request
            loginButton.interactable = false;

            Debug.Log("Sending POST request to: " + LOGIN_URL);

            // Send request
            yield return request.SendWebRequest();

            // Re-enable button
            loginButton.interactable = true;

            Debug.Log("Request sent. Result: " + request.result);

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login successful! Response: " + request.downloadHandler.text);
                
                // Clear input fields on success
                usernameInput.text = "";
                passwordInput.text = "";
            }
            else
            {
                Debug.LogError("Login failed: " + request.error);
                Debug.LogError("Response: " + request.downloadHandler.text);
            }
        }
    }

    

    [System.Serializable]
    public class LoginData
    {
        public string username;
        public string password;
    }
} 