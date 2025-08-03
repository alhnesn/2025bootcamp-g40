using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUIController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject orderPanel;
    public Transform ingredientContainer;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI priceText;
    public GameObject ingredientImagePrefab; // Simple Image component prefab
    
    [Header("Layout Settings")]
    public float panelWidth = 120f;
    public float ingredientSpacing = 10f;
    public float maxIngredientSize = 80f;
    public float screenMargin = 20f; // Margin from screen edges
    
    private Order currentOrder;
    private List<GameObject> ingredientImages = new List<GameObject>();
    private float orderStartTime;

    void Start()
    {
        HideOrder();
        SetupUIPositioning();
    }

    private void SetupUIPositioning()
    {
        SetupIngredientPanel();
        SetupPriceText();
        SetupTimerText();
    }

    private void SetupIngredientPanel()
    {
        if (orderPanel != null)
        {
            RectTransform panelRect = orderPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Anchor to left-center of screen
                panelRect.anchorMin = new Vector2(0f, 0.5f);
                panelRect.anchorMax = new Vector2(0f, 0.5f);
                panelRect.pivot = new Vector2(0f, 0.5f);
                
                // Position on left side with margin
                panelRect.anchoredPosition = new Vector2(screenMargin, 0f);
                
                // Set width, height will be dynamic based on ingredients
                panelRect.sizeDelta = new Vector2(panelWidth, 100f); // Height will be adjusted in SetupOrderUI
            }
        }
    }
    
    private void SetupPriceText()
    {
        if (priceText != null)
        {
            RectTransform priceRect = priceText.GetComponent<RectTransform>();
            if (priceRect != null)
            {
                // Anchor to bottom-left corner
                priceRect.anchorMin = new Vector2(0f, 0f);
                priceRect.anchorMax = new Vector2(0f, 0f);
                priceRect.pivot = new Vector2(0f, 0f);
                
                // Position at bottom-left with margin
                priceRect.anchoredPosition = new Vector2(screenMargin, screenMargin);
                
                // Set size
                priceRect.sizeDelta = new Vector2(100f, 30f);
            }
        }
    }
    
    private void SetupTimerText()
    {
        if (timerText != null)
        {
            RectTransform timerRect = timerText.GetComponent<RectTransform>();
            if (timerRect != null)
            {
                // Anchor to top-left corner
                timerRect.anchorMin = new Vector2(0f, 1f);
                timerRect.anchorMax = new Vector2(0f, 1f);
                timerRect.pivot = new Vector2(0f, 1f);
                
                // Position at top-left with margin
                timerRect.anchoredPosition = new Vector2(screenMargin, -screenMargin);
                
                // Set size
                timerRect.sizeDelta = new Vector2(100f, 30f);
            }
        }
    }
    
    public void ShowOrder(Order order)
    {
        Debug.Log("OrderUIController.ShowOrder() called");
    
        if (order == null)
        {
            Debug.LogError("Order is null!");
            return;
        }
        
        if (orderPanel == null)
        {
            Debug.LogError("OrderPanel is not assigned in OrderUIController!");
            return;
        }
        
        currentOrder = order;
        orderStartTime = Time.time;
        
        Debug.Log($"Setting up order UI for: {order.GetOrderDescription()}");
        SetupOrderUI();
        
        orderPanel.SetActive(true);
        
        // Make sure price and timer are also visible
        if (priceText != null) priceText.gameObject.SetActive(true);
        if (timerText != null) timerText.gameObject.SetActive(true);
        
        Debug.Log("Order panel should now be visible");
    }
    
    public void HideOrder()
    {
        Debug.Log("OrderUIController.HideOrder() called");
    
        if (orderPanel != null)
        {
            orderPanel.SetActive(false);
        }
        
        // Hide price and timer as well
        if (priceText != null) priceText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        
        ClearIngredientImages();
        currentOrder = null;
    }

    private void SetupOrderUI()
    {
        if (currentOrder == null) return;
        
        // Clear previous images
        ClearIngredientImages();
        
        // Set price (top)
        priceText.text = $"${currentOrder.totalPrice:F2}";
        
        // Get ingredients and REVERSE them for proper visual stacking
        List<string> ingredients = currentOrder.GetRequiredIngredients();
        List<string> reversedIngredients = new List<string>(ingredients);
        reversedIngredients.Reverse(); // Now BunTop will be at top of UI, BunBottom at bottom
        
        // Calculate ingredient size based on count
        float ingredientSize = CalculateIngredientSize(ingredients.Count);
        
        // Create ingredient images with reversed order
        CreateIngredientImages(reversedIngredients, ingredientSize);
        
        // Adjust panel height to fit ingredients
        AdjustPanelHeight(ingredients.Count, ingredientSize);
    }

    private float CalculateIngredientSize(int ingredientCount)
    {
        // Maximum available height (80% of screen height for safety)
        float maxAvailableHeight = Screen.height * 0.8f;
        
        // Calculate size if we use maximum size per ingredient
        float totalHeightIfMax = (ingredientCount * maxIngredientSize) + ((ingredientCount - 1) * ingredientSpacing);
        
        if (totalHeightIfMax <= maxAvailableHeight)
        {
            // We can use max size
            return maxIngredientSize;
        }
        else
        {
            // We need to shrink to fit
            float availableForIngredients = maxAvailableHeight - ((ingredientCount - 1) * ingredientSpacing);
            return Mathf.Max(30f, availableForIngredients / ingredientCount); // Minimum 30px size
        }
    }

    private void AdjustPanelHeight(int ingredientCount, float ingredientSize)
    {
        if (orderPanel != null)
        {
            RectTransform panelRect = orderPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                // Calculate total height needed for ingredients
                float totalHeight = (ingredientCount * ingredientSize) + ((ingredientCount - 1) * ingredientSpacing) + 20f; // 20f padding
                
                // Update panel height
                panelRect.sizeDelta = new Vector2(panelWidth, totalHeight);
            }
        }
    }

    private void CreateIngredientImages(List<string> ingredients, float imageSize)
    {
        for (int i = 0; i < ingredients.Count; i++)
        {
            GameObject imageObj = Instantiate(ingredientImagePrefab, ingredientContainer);
            Image image = imageObj.GetComponent<Image>();
            
            // Set sprite from database
            Sprite thumbnail = IngredientDatabaseManager.GetThumbnail(ingredients[i]);
            if (thumbnail != null)
            {
                image.sprite = thumbnail;
            }
            else
            {
                Debug.LogWarning($"No thumbnail found for ingredient: {ingredients[i]}");
            }
            
            // Set size
            RectTransform rectTransform = imageObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(imageSize, imageSize);
            
            ingredientImages.Add(imageObj);
        }
        
        // Set up vertical layout - ingredients flow top to bottom, centered
        VerticalLayoutGroup layoutGroup = ingredientContainer.GetComponent<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            layoutGroup.spacing = ingredientSpacing;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter; // Center the ingredients within the container
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.reverseArrangement = false;
        }
        
        // Make sure the ingredient container fills the panel and centers content
        if (ingredientContainer != null)
        {
            RectTransform containerRect = ingredientContainer.GetComponent<RectTransform>();
            if (containerRect != null)
            {
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.sizeDelta = Vector2.zero;
                containerRect.anchoredPosition = Vector2.zero;
            }
        }
    }

    private void ClearIngredientImages()
    {
        foreach (GameObject img in ingredientImages)
        {
            if (img != null)
                DestroyImmediate(img);
        }
        ingredientImages.Clear();
    }
    
    void Update()
    {
        if (currentOrder != null && orderPanel.activeInHierarchy)
        {
            UpdateTimer();
        }
    }
    
    private void UpdateTimer()
    {
        float elapsed = Time.time - orderStartTime;
        float remaining = Mathf.Max(0f, currentOrder.timeLimit - elapsed);
        
        int minutes = Mathf.FloorToInt(remaining / 60f);
        int seconds = Mathf.FloorToInt(remaining % 60f);
        
        timerText.text = $"{minutes:00}:{seconds:00}";
        
        // Color coding for urgency
        if (remaining < 30f)
        {
            timerText.color = Color.red;
        }
        else if (remaining < 60f)
        {
            timerText.color = Color.yellow;
        }
        else
        {
            timerText.color = Color.white;
        }
    }
}
