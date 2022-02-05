using UnityEngine;

// This component represents a spike hazard that will kill the player when touched.
public class Spikes : MonoBehaviour
{
    // If the thing that entered our trigger has a player controller, kill it.
    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<PlayerController>();
        if (player != null)
        {
            player.Die();
        }
    }
}
