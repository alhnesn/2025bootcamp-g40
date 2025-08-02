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
    public OrderType orderType;
    public string customerName;
    public float timeLimit = 60f;  // Time limit in seconds
    
    // Scoring weights (can be overridden by specific order types)
    public float unwantedIngredientPenalty = -20f;
    public float missingIngredientPenalty = -10f;
    public float wrongOrderPenalty = -5f;
    public float perfectBonus = 50f;
    
    public abstract void GenerateRandomOrder();
    public abstract float EvaluateOrder(GameObject deliveredPlate);
    public abstract string GetOrderDescription();
    public abstract List<string> GetRequiredIngredients();
}