using UnityEngine;
using UnityEngine.SceneManagement;

// This component represents a door opject that will teleport the player if they interact with it.
public class Door : MonoBehaviour
{
    [Tooltip("The name of the new scene we want to take the player to when they use this door.")]
    public string SceneName;

    // If the player is holding up while standing inside of the doors trigger, we warp them to the new scene.
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (SceneName != "")
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                if (Input.GetAxis("Vertical") > 0.2f || Input.GetKey(KeyCode.UpArrow))
                {
                    SceneManager.LoadScene(SceneName);
                }
            }
        }
    }
}
