using UnityEngine;

//This Component lets us move a given game object between point A and point B, represented by two other game objects.
public class Mover : MonoBehaviour
{
    [Tooltip("The Game Object representing the starting point.")]
    public GameObject PointA;

    [Tooltip("The Game Object representing the ending point.")]
    public GameObject PointB;

    [Tooltip("The Game Object that the mover will be moving.")]
    public GameObject Target;

    [Tooltip("How Far the object is on the track.")]
    public float delta;

    [Tooltip("Whether we are currently moving forwards or backwards.")]
    public bool goingUp;

    [Tooltip("How long we wait after it reaches the end of the track, before reversing.")]
    public float WaitTime;

    [Tooltip("How fast this object should move along the track")]
    public float Speed = 1.0f;

    void Update()
    {
        if(!goingUp)
        {
            delta += Time.deltaTime * Speed;
            if(delta > 1 + WaitTime/2)
            {
                goingUp = true;
            }
        }
        else
        {
            delta -= Time.deltaTime * Speed;
            if (delta < 0 - WaitTime / 2)
            {
                goingUp = false;
            }
        };

        Target.transform.position = Vector3.Lerp(PointA.transform.position, PointB.transform.position, delta);
    }
}
