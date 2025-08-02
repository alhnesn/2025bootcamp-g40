using UnityEngine;
using UnityEngine.UI;

public class Customer : MonoBehaviour
{
    [Header("Customer Settings")]
    public string customerName = "Customer";
    public float orderTimeLimit = 120f; // 2 minutes default
    
    [Header("UI References")]
    public Text orderDisplayText;
    public Text timerText;
    public Text scoreText;
    
    // Current order
    private Order currentOrder = null;
    private float orderTimer = 0f;
    private bool hasActiveOrder = false;
    
    // Scoring
    private float totalScore = 0f;


    void Start()
    {
        GenerateNewOrder();
    }
    
    void Update()
    {
        if (hasActiveOrder)
        {
            UpdateOrderTimer();
        }
    }

    public void GenerateNewOrder()
    {
        // For now, only generate hamburger orders
        // Later, you can add logic to randomly choose order types
        currentOrder = new HamburgerOrder();
        currentOrder.customerName = customerName;
        currentOrder.timeLimit = orderTimeLimit;
        currentOrder.GenerateRandomOrder();
        
        hasActiveOrder = true;
        orderTimer = 0f;
        
        UpdateOrderDisplay();
        
        Debug.Log($"{customerName} ordered: {currentOrder.GetOrderDescription()}");
    }

    public float DeliverOrder(GameObject deliveredPlate) // TODO: empty the plate
    {
        if (!hasActiveOrder || currentOrder == null)
        {
            Debug.Log("No active order to deliver!");
            return 0f;
        }
        
        float score = currentOrder.EvaluateOrder(deliveredPlate);
        
        // Time bonus/penalty
        float timeBonus = CalculateTimeBonus();
        score += timeBonus;
        
        totalScore += score;
        
        // Complete the order
        hasActiveOrder = false;
        
        Debug.Log($"Order delivered! Score: {score} (Time bonus: {timeBonus}) - Total score: {totalScore}");
        
        UpdateScoreDisplay();
        
        // Generate new order after a delay
        Invoke(nameof(GenerateNewOrder), 3f); // TODO: make him leave the restaurant
        
        return score;
    }

    private float CalculateTimeBonus()
    {
        if (currentOrder == null) return 0f;
        
        float timeRatio = orderTimer / currentOrder.timeLimit;
        
        if (timeRatio < 0.5f) // Delivered in first half of time
        {
            return 10f; // Time bonus
        }
        else if (timeRatio > 1.0f) // Delivered late
        {
            return -5f; // Time penalty
        }
        
        return 0f; // No bonus or penalty
    }

    private void UpdateOrderTimer()
    {
        orderTimer += Time.deltaTime;
        
        if (timerText != null)
        {
            float remainingTime = Mathf.Max(0f, currentOrder.timeLimit - orderTimer);
            timerText.text = $"Time: {remainingTime:F1}s";
        }
        
        // Check if time is up
        if (orderTimer >= currentOrder.timeLimit)
        {
            Debug.Log($"{customerName}'s order timed out!");
            hasActiveOrder = false;
            // Generate new order after timeout
            Invoke(nameof(GenerateNewOrder), 2f); // TODO: make him leave the restaurant
        }
    }
    
    private void UpdateOrderDisplay()
    {
        if (orderDisplayText != null && currentOrder != null)
        {
            orderDisplayText.text = currentOrder.GetOrderDescription();
        }
    }
    
    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {totalScore:F0}";
        }
    }
    
    // Public getters
    public bool HasActiveOrder() => hasActiveOrder;
    public Order GetCurrentOrder() => currentOrder;
    public float GetTotalScore() => totalScore;
}
