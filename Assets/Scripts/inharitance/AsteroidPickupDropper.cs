using UnityEngine;

public class AsteroidPickupDropper : MonoBehaviour
{
    [System.Serializable]
    public struct PickupDrop
    {
        public GameObject pickupPrefab;
        public float dropChance; // 0-100
    }

    public PickupDrop[] pickupDrops;
    [Range(0, 100)]
    public float overallDropChance = 40f;

    // <-- Updated method
    public void TryDropPickup(Vector3 position)
    {
        if (pickupDrops.Length == 0) return;

        float roll = Random.value * 100f; // 0-100 inclusive
        if (roll > overallDropChance) return; // respect overall drop chance

        // Weighted selection
        float totalWeight = 0f;
        foreach (var pd in pickupDrops) totalWeight += pd.dropChance;

        float selection = Random.value * totalWeight;
        float running = 0f;

        foreach (var pd in pickupDrops)
        {
            running += pd.dropChance;
            if (selection <= running)
            {
                if (pd.pickupPrefab != null)
                    Instantiate(pd.pickupPrefab, position, Quaternion.identity);
                return; // only spawn one pickup
            }
        }
    }
}