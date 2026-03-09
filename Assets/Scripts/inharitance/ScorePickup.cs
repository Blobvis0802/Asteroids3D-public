using UnityEngine;

public class ScorePickup : Pickup
{
    [SerializeField] private int amount = 50;

    public override void Activate(Player player)
    {
        if (PersistentScoreManager.Instance != null)
        {
            PersistentScoreManager.Instance.AddScore(amount);
        }

        base.Activate(player);
    }
}