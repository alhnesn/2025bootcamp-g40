// Order.cs (Base class for all order types)
using System.Collections.Generic;
using UnityEngine;

public enum OrderType
{
    Hamburger,
    Stew,      // Future
    Salad      // Future
}

public abstract class Order
{
    [Header("Basic Info")]
    public OrderType orderType;
    public string customerName;
    public float timeLimit;
    public float totalPrice;
    
    [Header("Scoring System (0-100)")]
    public float extraIngredientPenalty = -50f;
    public float missingIngredientPenalty = -30f;
    public float wrongOrderPenalty = -20f;
    public float perfectOrderScore = 90f;
    public float earlyDeliveryBonus = 10f;
    
    public abstract void GenerateRandomOrder();
    public abstract float EvaluateOrder(GameObject deliveredPlate);
    public abstract string GetOrderDescription();
    public abstract List<string> GetRequiredIngredients();
    public abstract float CalculateTotalPrice();
    public abstract float CalculateTotalTime();
}