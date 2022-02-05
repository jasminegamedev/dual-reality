using UnityEngine;

// This component represents a saw hazard that will rotate constantly, and kill the player when touched.
public class Saw : MonoBehaviour
{
    [Tooltip("Controls how fast the sawblade rotates.")]
    public float SpinSpeed = 1;

    private const float SPEED_ADJUSTMENT = 240;

    void Update()
    {
        transform.Rotate(transform.forward, SpinSpeed * SPEED_ADJUSTMENT * Time.deltaTime);
    }

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
