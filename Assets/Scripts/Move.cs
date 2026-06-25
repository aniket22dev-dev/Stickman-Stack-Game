using UnityEngine;


public class Move : MonoBehaviour
{
    public bool IsMove = true;
    public float speed;

    // Set by Spawn.cs at runtime:
    //  1 = coming from right → moves left  (original)
    // -1 = coming from left  → moves right (new)
    [HideInInspector] public float direction = 1f;

    void Update()
    {
        if (IsMove)
        {
            speed = Mathf.Min(speed, 0.7f); // hard cap
            transform.position -= new Vector3(direction * speed * Time.deltaTime, 0, 0);
        }
    }
}
