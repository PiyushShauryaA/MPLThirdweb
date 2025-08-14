using Thirdweb;
using Thirdweb;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ERC20Main : MonoBehaviour
{
   public static ERC20Main Instance;

    [SerializeField] private Button claimButton;
    [SerializeField] private Button getBalanceButton;
    [SerializeField] private TMP_Text balanceText;
    [SerializeField] private TMP_Text claimText;
    [SerializeField] private Button burnButton;
    [SerializeField] private TMP_Text burnText;

    [SerializeField] private string toAddress;
    [SerializeField] private TMP_Text transferText;

    [Header("PopUp Message Panel")]
    [SerializeField] private GameObject popUpMessage;
    [SerializeField] private TMP_Text popUpMessageText;

    [Header("Add Fund Wallet panel")]
    [SerializeField] private GameObject addFundWalletPanel;
    [SerializeField] private TMP_Text addFundWalletBalanceText;
    [SerializeField] private TMP_Text addFundWalletTokenBalanceText;

   
         private ThirdwebSDK sdk;

    private string tokenContractAddress = "0xb3d511F6506870E0F77f01938283F9f916b1FFC9";
    //private string tokenContractAddress = "0x0688977ae5b10075f46519063fd2f03adc052c1f";
    private string chainId = "1";

    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Check if ThirdwebManager is available
        if (ThirdwebManager.Instance == null)
        {
            Debug.LogError("ThirdwebManager.Instance is null! Make sure ThirdwebManager is in your scene.");
            return;
        }
        
        sdk = ThirdwebManager.Instance.SDK;
        
        // Check if SDK is available
        if (sdk == null)
        {
            Debug.LogError("ThirdwebManager.Instance.SDK is null!");
            return;
        }
        
        claimButton.onClick.AddListener(ClaimErc20Token);
        burnButton.onClick.AddListener(BurnErc20Token);
        getBalanceButton.onClick.AddListener(GetTokenBalance);
       // GetTokenBalance();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void GetTokenBalance()
    {
        Debug.Log("[balance btn clicked]");
        try
        {
            string address = await sdk.wallet.GetAddress();
            Contract contract = sdk.GetContract(tokenContractAddress, chainId);
            var balance = await contract.ERC20.BalanceOf(address);
            balanceText.text = balance.ToString();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error getting token balance: {e.Message}");
            balanceText.text = "Error: " + e.Message;
        }
    }

    public async void ClaimErc20Token()
    {
        //Debug.Log("[claim btn clicked]");
        Debug.Log("[claim btn clicked]");
       Contract contract = sdk.GetContract(tokenContractAddress, chainId);
       var result = await contract.ERC20.Claim("50");
       Debug.Log("Claimed ERC20 Token");
       claimText.text = "Claimed";

       GetTokenBalance();
    }

     public async void BurnErc20Token()
    {
        Debug.Log("[burn btn clicked]");
        try
        {
            Contract contract = sdk.GetContract(tokenContractAddress, chainId);
            var result = await contract.ERC20.Burn("1");
            Debug.Log("Burned ERC20 Token");
            burnText.text = "Burned";
            
            GetTokenBalance();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error burning ERC20 token: {e.Message}");
            burnText.text = "Error: " + e.Message;
        }
    }

    public async void TransferErc20Token()
    {
        Debug.Log("[transfer btn clicked]");
        try
        {
            Contract contract = sdk.GetContract(tokenContractAddress, chainId);
            var result = await contract.ERC20.Transfer(toAddress, "1");
            Debug.Log("Transferred ERC20 Token");
            transferText.text = "Transferred";
            GetTokenBalance();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error transferring ERC20 token: {e.Message}");
            transferText.text = "Error: " + e.Message;
        }
    }


public async void OnChessClick()
{
    Debug.Log("Chess button clicked!");
    
    try
    {
        // Check if ThirdwebManager is initialized
        if (ThirdwebManager.Instance == null)
        {
            Debug.LogError("ThirdwebManager.Instance is null!");
            return;
        }
        
        if (sdk == null)
        {
            Debug.LogError("ThirdwebManager.Instance.SDK is null!");
            return;
        }
        
        // Check if wallet is connected (properly await the async method)
        bool isConnected = await sdk.wallet.IsConnected();
        Debug.Log($"Wallet connected: {isConnected}");
        
        if (isConnected)
        {
            string address = await sdk.wallet.GetAddress();
            Debug.Log($"Connected wallet address: {address}");
            GetTokenBalance();
        }
        else
        {
            Debug.Log("No wallet connected");
            popUpMessage.SetActive(true);
            popUpMessageText.text = "No wallet connected";
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError($"Error in OnChessClick: {e.Message}");
    }
    
}



    public async void ConnectWallet()
    {
        Debug.Log("Connect wallet button clicked!");
        try
        {
            // Check if wallet is already connected
            bool isConnected = await sdk.wallet.IsConnected();
            if (isConnected)
            {
                Debug.Log("Wallet is already connected!");
                popUpMessage.SetActive(true);
                popUpMessageText.text = "Wallet already connected!";
                return;
            }

            // Connect to wallet using Embedded Wallet (you can change this to other providers)
            var connection = new WalletConnection(
                provider: WalletProvider.EmbeddedWallet,
                chainId: 1, // Ethereum mainnet
                email: "piyush@shauryainfosoft.com" // You might want to get this from UI input
            );

            string address = await sdk.wallet.Connect(connection);
            Debug.Log($"Connected to wallet: {address}");
            
            popUpMessage.SetActive(true);
            popUpMessageText.text = $"Connected: {address}";
            
            // Refresh balance after connection
            GetTokenBalance();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error connecting wallet: {e.Message}");
            popUpMessage.SetActive(true);
            popUpMessageText.text = "Connection failed: " + e.Message;
        }
    }

    public async void FundWallet(int val)
    {
         bool isConnected = await sdk.wallet.IsConnected();
            if (!isConnected)
            {
                Debug.Log("Please connect your wallet first!");
                popUpMessage.SetActive(true);
                popUpMessageText.text = "Please connect your wallet first!";
                return;
            }
        try
        {
            // Check if ThirdwebManager is initialized
            

            // Get the active wallet
            var wallet = ThirdwebManager.Instance.SDK.wallet;
            if (wallet == null)
            {
                Debug.LogError("No active wallet found!");
                return;
            }

            // Get the wallet address
            string walletAddress = await wallet.GetAddress();
            if (string.IsNullOrEmpty(walletAddress))
            {
                Debug.LogError("Failed to get wallet address!");
                return;
            }
             // Create the funding URL
            string fullUrl =null;

            switch (val)
            {
                case 5:
                    fullUrl = $"https://thirdweb.com/pay?amount=5000000&chainId=1&clientId=d391b93f5f62d9c15f67142e43841acc&recipientAddress={walletAddress}&tokenAddress=0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48";

                    break;
                case 10:
                     fullUrl = $"https://thirdweb.com/pay?amount=10000000&chainId=1&clientId=d391b93f5f62d9c15f67142e43841acc&recipientAddress={walletAddress}&tokenAddress=0xA0b86991c6218b36c1d19D4a2e9Eb0cE3606eB48";

                    break;
                
            }
           
            // Open the URL in the default browser
            Application.OpenURL(fullUrl);
            
            Debug.Log($"Opening funding URL: {fullUrl}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error in FundWallet: {ex.Message}");
        }
    }


public async void BuyToken(string val)
    {
        bool isConnected = await sdk.wallet.IsConnected();
            if (!isConnected)
            {
                Debug.Log("Please connect your wallet first!");
                popUpMessage.SetActive(true);
                popUpMessageText.text = "Please connect your wallet first!";
                return;
            }

        //Debug.Log("[claim btn clicked]");
        Debug.Log("[claim btn clicked]");
       Contract contract = sdk.GetContract(tokenContractAddress, chainId);
       var result = await contract.ERC20.Claim(val);
       Debug.Log("Claimed ERC20 Token");
       claimText.text = "Claimed";

       ShowWalletTokenBalance();
    }

public async void ShowWalletTokenBalance()
{
    string address = await sdk.wallet.GetAddress();
    addFundWalletBalanceText.text = address;
    Contract contract = sdk.GetContract(tokenContractAddress, chainId);
    var balance = await contract.ERC20.BalanceOf(address);
    addFundWalletTokenBalanceText.text = balance.ToString();
}
}
