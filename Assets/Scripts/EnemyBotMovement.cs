using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BotPlayerController2D : MonoBehaviour
{
    [Header("Movement")]
    public float moveForce = 25f;
    public float maxSpeed = 9f;
    public float brakeForce = 8f;

    [Header("Steering")]
    public float turnSpeed = 220f;
    public float minTurnSpeed = 80f; // turn speed at max velocity

    Rigidbody2D rb;

    void Awake() => rb = GetComponent<Rigidbody2D>();

    void FixedUpdate()
    {
        float throttle = Input.GetAxisRaw("Vertical");
        float turn = -Input.GetAxisRaw("Horizontal");

        float speedFactor = rb.velocity.magnitude / maxSpeed;
        float effectiveTurn = Mathf.Lerp(turnSpeed, minTurnSpeed, speedFactor);
        rb.MoveRotation(rb.rotation + turn * effectiveTurn * Time.fixedDeltaTime);

        // Throttle
        if (throttle != 0)
        {
            rb.AddForce((Vector2)transform.up * throttle * moveForce);
        }
        else
        {
            rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, brakeForce * Time.fixedDeltaTime);
        }

        // Clamp speed
        if (rb.velocity.magnitude > maxSpeed)
            rb.velocity = rb.velocity.normalized * maxSpeed;
    }
}