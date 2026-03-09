using UnityEngine;

public class PowerSurgePickup : Pickup
{
    [SerializeField] private float surgeDuration = 5f;

    public override void Activate(Player player)
    {
        if (player != null)
        {
            player.StartPowerSurge(surgeDuration);
        }

        base.Activate(player);
    }
}