using UnityEngine;

public class Ladder : Building
{
    [SerializeField] float climbSpeed = 5f;
    
    void OnCollisionStay(Collision other)
    {
        Rigidbody rb = other.collider.attachedRigidbody;
        if (rb == null) return;
        
        // Get input (adjust based on your input system)
        float vertical = Input.GetAxis("Vertical");
        
        // Override gravity while on ladder
        rb.useGravity = false;
        
        // Apply climb velocity directly
        Vector3 climbVelocity = transform.up * vertical * climbSpeed * Time.deltaTime;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbVelocity.y, rb.linearVelocity.z);
    }
    
    void OnCollisionExit(Collision other)
    {
        Rigidbody rb = other.collider.attachedRigidbody;
        if (rb != null) rb.useGravity = true;
    }
}