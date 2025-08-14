using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MobileManager : MonoBehaviour
{
    public static MobileManager Instance;
    [Header("SMS API Settings")]
    [SerializeField] private bool enableSMS = false; // Set to false to prevent game pause
    [SerializeField] private string apiKey = "XQEnkDGY3vsefy5rgt8L4OidMT2071AKHcZRV9uawqpNozhIUxqVN8SEcsHWKnwQLU03ZhvJbC2t7Afp";
    [SerializeField] private string senderId = "FSTSHA";
    [SerializeField] private string messageId = "183105"; // DLT template ID
    [SerializeField] private string templateId = "YOUR_TEMPLATE_ID";
    [SerializeField] private string entityId = "YOUR_ENTITY_ID";
    [SerializeField] private string route = "dlt_manual";
    
    public string currentOtp;
    public string GetCurrentOtp() { 
        currentOtp = "123456"; //Random.Range(100000, 999999).ToString();
        return currentOtp; 
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SendSMS(string mobileNo)
    {
        if (!enableSMS)
        {
            Debug.Log("[MobileManager] SMS is disabled. Skipping SMS send.");
            return;
        }

        Debug.Log($"[MobileManager] SendSMS called. Mobile: {mobileNo}, OTP: {currentOtp}");
        StartCoroutine(SendSMSCoroutine(mobileNo, currentOtp));
    }

    // Non-blocking SMS method to prevent game pause
    public void SendSMSNonBlocking(string mobileNo)
    {
        if (!enableSMS)
        {
            Debug.Log("[MobileManager] SMS is disabled. Skipping SMS send.");
            return;
        }

        Debug.Log($"[MobileManager] SendSMSNonBlocking called. Mobile: {mobileNo}, OTP: {currentOtp}");
        
        #if UNITY_EDITOR
        Debug.Log($"[MobileManager] In Editor - SMS would be sent to {mobileNo} with OTP: {currentOtp}");
        #else
        StartCoroutine(SendSMSCoroutine(mobileNo, currentOtp));
        #endif
    }

    IEnumerator SendSMSCoroutine(string mobileNo, string otp)
    {
        Debug.Log($"[MobileManager] Starting SMS coroutine. Mobile: {mobileNo}, OTP: {otp}");
        
        // Validate mobile number
        if (string.IsNullOrEmpty(mobileNo) || mobileNo.Length != 10)
        {
            Debug.LogWarning($"[MobileManager] Invalid mobile number: {mobileNo}");
            yield break;
        }

        // Try simple SMS format first (more reliable)
        string simpleMessage = $"Your OTP is: {otp}. Valid for 10 minutes.";
        string simpleUrl = $"https://www.fast2sms.com/dev/bulkV2?authorization={apiKey}&route=q&message={UnityWebRequest.EscapeURL(simpleMessage)}&flash=0&numbers={mobileNo}";

        UnityWebRequest request = UnityWebRequest.Get(simpleUrl);
        request.SetRequestHeader("cache-control", "no-cache");
        request.timeout = 10;
        
        Debug.Log("Trying simple SMS URL: " + simpleUrl);
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("SMS sent successfully: " + request.downloadHandler.text);
        }
        else
        {
            // Use LogWarning instead of LogError to prevent game pause
            Debug.LogWarning($"[MobileManager] Simple SMS failed: {request.error}");
            Debug.LogWarning($"[MobileManager] Response Code: {request.responseCode}");
            
            // Try DLT format as fallback
            if (request.responseCode == 400)
            {
                Debug.LogWarning("[MobileManager] Trying DLT format as fallback...");
                yield return TryDLTFormat(mobileNo, otp);
            }
        }

        if (request != null)
        {
            request.Dispose();
        }
    }

    // Fallback DLT format method
    private IEnumerator TryDLTFormat(string mobileNo, string otp)
    {
        string dltUrl = $"https://www.fast2sms.com/dev/bulkV2?authorization={apiKey}&route=dlt&sender_id={senderId}&message={messageId}&variables_values={otp}%7C&flash=0&numbers={mobileNo}&schedule_time=";

        UnityWebRequest request = UnityWebRequest.Get(dltUrl);
        request.SetRequestHeader("cache-control", "no-cache");
        request.timeout = 10;
        
        Debug.Log("Trying DLT URL: " + dltUrl);
        
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("DLT SMS sent successfully: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogWarning($"[MobileManager] DLT SMS also failed: {request.error}");
            Debug.LogWarning($"[MobileManager] Response Code: {request.responseCode}");
            Debug.LogWarning($"[MobileManager] Response: {request.downloadHandler?.text}");
            
            if (request.responseCode == 400)
            {
                Debug.LogWarning("[MobileManager] 400 Bad Request - Possible causes:");
                Debug.LogWarning("1. Invalid API key");
                Debug.LogWarning("2. Invalid sender ID");
                Debug.LogWarning("3. DLT template not approved");
                Debug.LogWarning("4. API quota exceeded");
                Debug.LogWarning("5. Account not active");
            }
        }

        if (request != null)
        {
            request.Dispose();
        }
    }

    // Public method to toggle SMS functionality
    public void SetSMSEnabled(bool enabled)
    {
        enableSMS = enabled;
        Debug.Log($"[MobileManager] SMS functionality {(enabled ? "enabled" : "disabled")}");
    }

    // Public method to check if SMS is enabled
    public bool IsSMSEnabled()
    {
        return enableSMS;
    }
}
