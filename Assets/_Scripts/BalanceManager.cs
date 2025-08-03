using UnityEngine;
using TMPro;

public class BalanceManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI balanceText;
    
    [Header("Balance Settings")]
    public float startingBalance = 0f;
    public string balancePrefix = "$";
    public int decimalPlaces = 2;
    
    private static BalanceManager instance;
    private float currentBalance;

    // Singleton access
    public static BalanceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<BalanceManager>();
                if (instance == null)
                {
                    Debug.LogError("No BalanceManager found in scene! Please add one to your UI.");
                }
            }
            return instance;
        }
    }

    public float CurrentBalance => currentBalance;
    
    void Awake()
    {
        // Ensure singleton
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Initialize balance
        currentBalance = startingBalance;
    }

    void Start()
    {
        SetupBalanceUI();
        UpdateBalanceDisplay();
    }
    
    private void SetupBalanceUI()
    {
        if (balanceText != null)
        {
            RectTransform balanceRect = balanceText.GetComponent<RectTransform>();
            if (balanceRect != null)
            {
                // Anchor to top-right corner
                balanceRect.anchorMin = new Vector2(1f, 1f);
                balanceRect.anchorMax = new Vector2(1f, 1f);
                balanceRect.pivot = new Vector2(1f, 1f);
                
                // Position at top-right with margin
                balanceRect.anchoredPosition = new Vector2(-20f, -20f);
                
                // Set size
                balanceRect.sizeDelta = new Vector2(150f, 40f);
            }
        }
    }

    /// <summary>
    /// Add money to the player's balance
    /// </summary>
    /// <param name="amount">Amount to add (can be negative for spending)</param>
    public void AddMoney(float amount)
    {
        float oldBalance = currentBalance;
        currentBalance += amount;
        
        // Prevent negative balance (optional - remove if you want debt)
        currentBalance = Mathf.Max(0f, currentBalance);
        
        UpdateBalanceDisplay();
        
        // Log the transaction
        if (amount > 0)
        {
            Debug.Log($"Money earned: ${amount:F2} | Balance: ${oldBalance:F2} → ${currentBalance:F2}");
        }
        else if (amount < 0)
        {
            Debug.Log($"Money spent: ${Mathf.Abs(amount):F2} | Balance: ${oldBalance:F2} → ${currentBalance:F2}");
        }
    }
    
    /// <summary>
    /// Spend money from the player's balance
    /// </summary>
    /// <param name="amount">Amount to spend</param>
    /// <returns>True if transaction successful, false if insufficient funds</returns>
    public bool SpendMoney(float amount)
    {
        if (currentBalance >= amount)
        {
            AddMoney(-amount);
            return true;
        }
        else
        {
            Debug.LogWarning($"Insufficient funds! Trying to spend ${amount:F2}, but only have ${currentBalance:F2}");
            return false;
        }
    }

    /// <summary>
    /// Set the balance to a specific amount
    /// </summary>
    /// <param name="amount">New balance amount</param>
    public void SetBalance(float amount)
    {
        currentBalance = Mathf.Max(0f, amount);
        UpdateBalanceDisplay();
        Debug.Log($"Balance set to: ${currentBalance:F2}");
    }
    
    private void UpdateBalanceDisplay()
    {
        if (balanceText != null)
        {
            balanceText.text = $"{balancePrefix}{currentBalance.ToString($"F{decimalPlaces}")}";
        }
    }
    
    /// <summary>
    /// Reset balance to starting amount (useful for testing or new game)
    /// </summary>
    public void ResetBalance()
    {
        SetBalance(startingBalance);
    }
    
    // Optional: Save/Load balance (for future persistence)
    public void SaveBalance()
    {
        PlayerPrefs.SetFloat("PlayerBalance", currentBalance);
        PlayerPrefs.Save();
    }
    
    public void LoadBalance()
    {
        if (PlayerPrefs.HasKey("PlayerBalance"))
        {
            currentBalance = PlayerPrefs.GetFloat("PlayerBalance");
            UpdateBalanceDisplay();
        }
    }

}
