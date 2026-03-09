using UnityEngine;

public class HealthPickup : Pickup
{
    [SerializeField] private int healthAmount = 1;

    public override void Activate(Player player)
    {
        if (game != null)
        {
            game.AddLives(healthAmount);
        }

        base.Activate(player);
    }
}