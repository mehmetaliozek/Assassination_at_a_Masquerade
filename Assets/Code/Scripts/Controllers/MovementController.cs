using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovementController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float speed;

    public Vector3 Velocity => rb.linearVelocity;

    public float MagnitudeVelocity => rb.linearVelocity.magnitude;

    public void Move(float x, float z)
    {
        Vector3 move = new Vector3(x * speed, 0, z * speed / 2);
        Vector3 y = new Vector3(0, rb.linearVelocity.y, 0);

        rb.linearVelocity = (move + y);
        SetDirection();
    }

    private void SetDirection()
    {
        if (rb.linearVelocity.x < 0)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else if (rb.linearVelocity.x > 0)
        {
            transform.localScale = new Vector3(1, 1, 1);
        }
    }
}