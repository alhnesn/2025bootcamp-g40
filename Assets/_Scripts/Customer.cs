using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
        Debug.Log($"{customerName} is waiting for order to be taken");
    }
    
    void Update()
    {
        if (hasActiveOrder)
        {
            UpdateOrderTimer();
        }
    }

    public void TakeOrder()
    {
        Debug.Log($"Customer.TakeOrder() called for {customerName}");
    
        if (hasActiveOrder)
        {
            Debug.Log("Customer already has an active order!");
            return;
        }
        
        GenerateNewOrder();
        Debug.Log($"Generated new order: {currentOrder?.GetOrderDescription()}");
        
        // Show order UI
        OrderUIController orderUI = FindAnyObjectByType<OrderUIController>();
        if (orderUI == null)
        {
            Debug.LogError("OrderUIController not found in scene!");
            return;
        }
        
        Debug.Log("Found OrderUIController, calling ShowOrder()");
        orderUI.ShowOrder(currentOrder);
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

    public float DeliverOrder(GameObject deliveredPlate)
    {
        if (!hasActiveOrder || currentOrder == null)
        {
            Debug.Log("No active order to deliver!");
            return 0f;
        }
        
        float score = currentOrder.EvaluateOrder(deliveredPlate);
        
        // Early delivery bonus
        float timeRatio = orderTimer / currentOrder.timeLimit;
        if (timeRatio < 0.5f) // Delivered in first half of time
        {
            score += currentOrder.earlyDeliveryBonus;
            Debug.Log($"Early delivery bonus: +{currentOrder.earlyDeliveryBonus}");
        }
        
        // Ensure score is 0-100
        score = Mathf.Clamp(score, 0f, 100f);
        
        // Calculate payment based on score
        float payment = (score / 100f) * currentOrder.totalPrice;
        
        totalScore += score;
    
        // Empty the plate - destroy all ingredients
        EmptyPlate(deliveredPlate);
        
        Debug.Log($"Order delivered! Score: {score}/100, Payment: ${payment:F2} (Full price: ${currentOrder.totalPrice:F2})");
        Debug.Log($"Total score: {totalScore}");
        
        // Complete the order
        hasActiveOrder = false;
        
        UpdateScoreDisplay();

        OrderUIController orderUI = FindAnyObjectByType<OrderUIController>();
        if (orderUI != null)
        {
            orderUI.HideOrder();
        }
        
        // Destroy customer after 5 seconds
        Destroy(gameObject, 5f);
        
        return payment; // Return payment instead of score
    }

    private void EmptyPlate(GameObject plate)
    {
        Container container = plate.GetComponent<Container>();
        if (container == null) return;
        
        // Get all items and destroy them
        List<GameObject> items = container.GetAllItems();
        foreach (GameObject item in items)
        {
            container.TakeTopItem(); // Remove from container
            Destroy(item); // Destroy the ingredient
        }
        
        Debug.Log("Plate emptied - all ingredients destroyed");
    }

    private void UpdateOrderTimer()
    {
        orderTimer += Time.deltaTime;
    
        if (timerText != null)
        {
            float remainingTime = Mathf.Max(0f, currentOrder.timeLimit - orderTimer);
            timerText.text = $"Time: {remainingTime:F1}s";
        }
        
        // Check if time is up - customer leaves
        if (orderTimer >= currentOrder.timeLimit)
        {
            Debug.Log($"{customerName}'s order timed out! Customer is leaving.");
            hasActiveOrder = false;
            
            // Hide order UI
            OrderUIController orderUI = FindAnyObjectByType<OrderUIController>();
            if (orderUI != null)
            {
                orderUI.HideOrder();
            }

            // Customer leaves immediately (destroy)
            Destroy(gameObject, 1f);
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
