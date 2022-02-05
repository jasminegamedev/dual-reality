using UnityEngine;

// This compomponent allows us to clamp the Camera to the X and/or Y axis at a certain position. This allows us to have greater control over how screen scrolling works, and makes it so we can lock the camera so it's not moving around as much.
public class CameraCustom : MonoBehaviour
{
    [Tooltip("This vector represents the X and/or Y position you want to clamp the camera to.")]
    public Vector2 ClampPosition;

    [Tooltip("True if we want to clamp the camera position on the X Axis.")]
    public bool ClampX;

    [Tooltip("True if we want to clamp the camera position on the Y Axis.")]
    public bool ClampY;

    void LateUpdate()
    {
        var x = ClampX ? ClampPosition.x : transform.position.x;
        var y = ClampY ? ClampPosition.y : transform.position.y;
        transform.position = new Vector3(x, y, transform.position.z);
    }
}
