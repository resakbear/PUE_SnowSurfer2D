using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [SerializeField] private float speed = 6f;                 // Speed going uphill (towards the player)
    [SerializeField] private float activationDistance = 30f;   // Starts moving when the player is this close
    [SerializeField] private float groundCheckDistance = 1.5f; // Raycast length to detect the floor
    [SerializeField] private LayerMask floorLayer;             // Select the "Floor" layer in the Inspector

    Rigidbody2D rb;
    Transform player;
    bool isActive;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // We rotate it ourselves to follow the slope

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null) player = playerObject.transform;
    }

    void FixedUpdate()
    {
        // Wait until the player is near, so the enemy doesn't walk away before we see it
        if (!isActive)
        {
            if (player == null) return;
            if (Vector2.Distance(player.position, rb.position) > activationDistance) return;
            isActive = true;
        }

        MoveAlongFloor();
    }

    /// <summary>
    /// Moves the enemy along the surface of the floor, in the opposite direction of the player (to the left).
    /// </summary>
    private void MoveAlongFloor()
    {
        RaycastHit2D hit = Physics2D.Raycast(rb.position, Vector2.down, groundCheckDistance, floorLayer);

        if (hit)
        {
            // Direction parallel to the floor, pointing left
            Vector2 tangent = new Vector2(-hit.normal.y, hit.normal.x);
            rb.linearVelocity = tangent * speed;

            // Tilt the enemy to match the slope
            float targetAngle = Mathf.Atan2(hit.normal.y, hit.normal.x) * Mathf.Rad2Deg - 90f;
            rb.rotation = Mathf.LerpAngle(rb.rotation, targetAngle, 10f * Time.fixedDeltaTime);
        }
        // If there is no floor below (in the air), gravity takes care of it
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.rigidbody != null && collision.rigidbody.CompareTag("Player"))
        {
            CrashDetector crashDetector = collision.rigidbody.GetComponentInChildren<CrashDetector>();
            if (crashDetector != null)
            {
                crashDetector.Crash();
            }
        }
    }

    // Draws the floor raycast in the Scene view to help adjust groundCheckDistance
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
