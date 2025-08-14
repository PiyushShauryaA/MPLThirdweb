using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking; // Added for UnityWebRequest
using Thirdweb.Unity.Examples;
//using GameScripts;

public class MainC : MonoBehaviour
{
    public string emailM = null;
    public string nameM = null;
    public string mobileM = null;
    public string imageUrlM = null;
    public string addressM;
    public string balanceM;

    public static MainC Instance;
    [Header("Login Mobile Number UI")]
    public GameObject mobilePanel;
    public TMP_InputField playerNameInput;
    public TMP_InputField mobileNumberInput;
    public Button mobileSubmitButton;


    [Header("Login OTP UI")]
    public GameObject otpPanel;
    public TMP_InputField otpInput;
    public Button otpSubmitButton;

    [Header("HomeScreen Welcome UI")]
    public GameObject homeScreenPanel;
    public TextMeshProUGUI homeScreenText;
    public Sprite profileSprite;
    public Button playerProfileBtn;
    public GameObject gameImgPrefab;
    public Transform gameImgParent;

    [Header("Player Profile UI")]
    public GameObject playerDataPanel;
    public Image playerDataProfileImg;
    public TextMeshProUGUI playerDataNameText;
    public TextMeshProUGUI playerDataMobileText;
    //public Button playerBackButton;
    //public Button InputFieldSubmitButton;

    [Header("Scene Names")]
    public string welcomeSceneName = "Welcome";
    public string thirdWebSceneName = "Scene_Playground";

    private const string SavedMobileKey = "SavedMobileNumber";
    private const string SavedNameKey = "SavedName";
    private const string SavedProfileIndex = "SavedProfileIndex";
    private string tempMobile = "";
    public int gameMode = 0;

    public GameObject externalPanel;
    
    
    
    [Header("Debug/Reset")]
    public Button resetButton; // Add this in Inspector for manual reset


    void Awake()
    {

        Instance = this;
    }

    void Start()
    {
        Debug.Log($"MainC Start() called - GameObject: {gameObject.name}, Active: {gameObject.activeInHierarchy}");
        
        initializedAll();
    }
    void initializedAll()
    {
        // Add listeners (remove first to avoid duplicates)
        if (mobileSubmitButton != null)
        {
            mobileSubmitButton.onClick.RemoveAllListeners();
            mobileSubmitButton.onClick.AddListener(OnMobileSubmit);
        }
        if (otpSubmitButton != null)
        {
            otpSubmitButton.onClick.RemoveAllListeners();
            otpSubmitButton.onClick.AddListener(OnOtpSubmit);
        }
        if (mobileNumberInput != null)
        {
            mobileNumberInput.characterLimit = 10;
            mobileNumberInput.onValueChanged.AddListener(delegate { LimitmobileNumberInput(); });
        }
        //InputFieldSubmitButton.onClick.RemoveAllListeners();
            //InputFieldSubmitButton.onClick.AddListener(() => HandleEmailLoginAsync()) ;
    }

    // Helper to enforce only 10 digits in mobileNumberInput
    void LimitmobileNumberInput()
    {
        if (mobileNumberInput != null && mobileNumberInput.text.Length > 10)
        {
            mobileNumberInput.text = mobileNumberInput.text.Substring(0, 10);
        }
    }

    void LimitplayerNameInput()
    {
        if (playerNameInput != null && playerNameInput.text.Length > 10)
        {
            playerNameInput.text = playerNameInput.text.Substring(0, 10);
        }
    }

    // Helper to enforce only 6 digits in otpInput
    void LimitOtpInput()
    {
        if (otpInput != null && otpInput.text.Length > 6)
        {
            otpInput.text = otpInput.text.Substring(0, 6);
        }
    }

    

    void SetupThirdWebScene()
    {
        Debug.Log("ThirdWeb scene initialized");
        // Add any ThirdWeb specific initialization here
    }

    // Panel control helper
   
    

    void ShowOtpPanel()
    {
        otpInput.text = MobileManager.Instance.GetCurrentOtp();
        Debug.Log(" OTP. Try -----------." + otpInput.text);
        otpPanel.SetActive(true);

    }

    void ShowHomeScreenPanel()
    {
        Debug.Log(" OTP. Try -----------.");
        
        {
            string mobile = PlayerPrefs.GetString(SavedMobileKey, "Unknown");
            string name = PlayerPrefs.GetString(SavedNameKey, "Unknown");
            //homeScreenText.text = $"Select Your Game!\nYour mobile: {mobile}\nName : {name}";
            name = name;
            mobile = mobile;
        }

        homeScreenPanel.SetActive(true);
        
    }

    public void OnMobileSubmit()
    {
         Debug.Log("Please enter a valid 10-digit mobile number.");
        if (mobileNumberInput == null) return;
        tempMobile = mobileNumberInput.text.Trim();
        if (string.IsNullOrEmpty(tempMobile) || tempMobile.Length != 10)
        {
            Debug.LogWarning("Please enter a valid 10-digit mobile number.");
            return;
        }
        gameMode = 1;
        // Optionally: validate mobile format here
        ShowOtpPanel();
    }

    public void OnOtpSubmit()
    {
        if (otpInput == null) return;
        string otp = otpInput.text.Trim();
        // Check for exactly 6 digits, all numeric
        if (otp.Length != 6 || !IsAllDigits(otp))
        {
            Debug.LogWarning("Please enter a valid 6-digit OTP.");
            otpInput.text = "";
            return;
        }
        string CorrectOtp = MobileManager.Instance.GetCurrentOtp().ToString();
        Debug.Log("000000000000000000 OTP. Try again." + otp);
        // Simulate OTP validation (replace with real logic as needed)
        if (otp == CorrectOtp)
        {
            PlayerPrefs.SetString(SavedMobileKey, tempMobile);

            profileSprite = ScrollSelectionController.Instance.imgProfile[ScrollSelectionController.Instance.currentIndex];
            Debug.Log("Profile Sprite: " + profileSprite.name);
            playerProfileBtn.image.sprite = profileSprite;
            PlayerPrefs.Save();
            // Call web service after saving
            string playerName = playerNameInput != null ? playerNameInput.text.Trim() : "";
            PlayerPrefs.SetString(SavedNameKey, playerName);
            PlayerPrefs.Save();
            string phone = tempMobile;
            StartCoroutine(SendAccountWebService(playerName, phone));
            
            // Use non-blocking SMS method to prevent game pause
            try
            {
                MobileManager.Instance.SendSMSNonBlocking(tempMobile);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[MainC] SMS error (non-blocking): {e.Message}");
            }

            ShowHomeScreenPanel();
        }
        else
        {
            Debug.LogWarning("Invalid OTP. Try again.");
            otpInput.text = ""; // Clear the field for retry
        }
    }

    IEnumerator SendAccountWebService(string name, string phone)
    {
        //string url = $"http://localhost:13756/account?name={UnityWebRequest.EscapeURL(name)}&phone={UnityWebRequest.EscapeURL(phone)}";
        string url = $"http://localhost:3000/account?name={UnityWebRequest.EscapeURL(name)}&phone={UnityWebRequest.EscapeURL(phone)}";
        Debug.Log($"Calling web service: {url}");
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Web service response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogWarning($"Web service error: {request.error}");
            }
        }
    }

    // Helper to check if a string is all digits
    bool IsAllDigits(string s)
    {
        foreach (char c in s)
        {
            if (!char.IsDigit(c)) return false;
        }
        return true;
    }

    public void LoadThirdWebScene()
    {
        Debug.Log("Loading ThirdWeb scene...");
        SceneManager.LoadScene(thirdWebSceneName);
    }

    public void LoadWelcomeScene()
    {
        Debug.Log("Loading Welcome scene...");
        SceneManager.LoadScene(welcomeSceneName);
    }

    // Public function to reset all PlayerPrefs data
    public void ResetAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        
        // Reset local variables to ensure clean state
        emailM = null;
        nameM = null;
        mobileM = null;
        imageUrlM = null;
        addressM = null;
        balanceM = null;
        tempMobile = "";
        gameMode = 0;
        
        Debug.Log("All PlayerPrefs data and local variables have been reset.");
    }
      
    // Static method that can be called from Console for debugging
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DebugReset()
    {
        if (Instance != null)
        {
            Debug.Log("Debug reset called from Console");
            Instance.ForceReset();
        }
        else
        {
            Debug.LogError("MainC Instance is null - cannot perform debug reset");
        }
    }

    void Update()
    {
        // Debug: Check if Update is being called (less frequent to avoid spam)
        
        
        // Press R to reset PlayerPrefs and reload scene if on welcome back
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R key pressed - Resetting PlayerPrefs and reloading scene");
            ResetAllPlayerPrefs();
            
            // Check if homeScreenPanel exists and is active before trying to access it
            if (homeScreenPanel != null && homeScreenPanel.activeSelf)
            {
                Debug.Log("Home screen panel is active, reloading scene");
                homeScreenPanel.SetActive(false);
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                Debug.Log("Home screen panel is not active or null, reloading scene anyway");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
        
        
    }
    
    // Manual reset function for UI button
    public void OnResetButtonClick()
    {
        Debug.Log("Reset button clicked - Resetting PlayerPrefs and reloading scene");
        ForceReset();
    }
    
    // Public method to force reset from anywhere
    public void ForceReset()
    {
        Debug.Log("Force reset called - Resetting PlayerPrefs and reloading scene");
        ResetAllPlayerPrefs();
        
        // Check if homeScreenPanel exists and is active before trying to access it
        if (homeScreenPanel != null && homeScreenPanel.activeSelf)
        {
            Debug.Log("Home screen panel is active, reloading scene");
            homeScreenPanel.SetActive(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.Log("Home screen panel is not active or null, reloading scene anyway");
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    
    void OnPlayerProfileBackButtonClick(string message)
    {
        profileSprite = ScrollSelectionController.Instance.imgProfile[PlayerPrefs.GetInt(SavedProfileIndex)];

        Debug.Log(message);

    }
    public void ShowPlayerProfile()
    {
        playerDataPanel.SetActive(true);
        Debug.Log("Player Profile");
        playerDataNameText.text = nameM;
       // playerDataMobileText.text = $"Wallet Address: {addressM}\nWallet Balance: {balanceM}\nSocial Login Mobile: {mobileM}\nSocial Login Name: {nameM}"; 

        profileSprite = ScrollSelectionController.Instance.imgProfile[ScrollSelectionController.Instance.currentIndex];
        Debug.Log("Profile Sprite: " + name);
        playerDataProfileImg.sprite = profileSprite;
       // SetProfileImage(imageUrlM);
    }
    public void passData(string name, string address, string balance, string email)
    {
        // Example: set UI fields or store the data as needed
        playerDataNameText.text = name;
        playerDataMobileText.text = $"Wallet Address: {address}\nWallet Balance: {balance}\nSocial Login Email: {email}";
        // You can add more logic here as needed
    }
    public void SetProfileImage(string imageUrl)
    {
        if (imageUrl == null)
        {
            return;
        }
        StartCoroutine(DownloadImage(imageUrl));
    }

    private System.Collections.IEnumerator DownloadImage(string imageUrl)
    {
        using (UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download image: " + uwr.error);
            }
            else
            {
                Texture2D texture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                // Set the sprite on the correct Image component

                playerProfileBtn.image.sprite = sprite;


                playerDataProfileImg.sprite = sprite;
            }
        }
    }
    
    
}
