using UnityEngine;

[System.Serializable]
public class PickupDrop
{
    [Header("Pickup Info")]
    public string name;

    [Header("Pickup Prefab")]
    public GameObject pickupPrefab;

    [Header("Drop Chance (%)")]
    [Range(0f, 100f)]
    public float dropChance = 10f;
}