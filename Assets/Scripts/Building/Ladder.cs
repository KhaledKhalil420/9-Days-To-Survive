using UnityEngine;
using System.Collections.Generic;

public class Ladder : Building
{
    [SerializeField] float climbSpeed = 5f;
    
    // List to track all rigidbodies currently on the ladder
    private List<Rigidbody> rigidbodiesOnLadder = new List<Rigidbody>();

    void OnCollisionStay(Collision other)
    {
        Rigidbody rb = other.collider.attachedRigidbody;
        if (rb == null) return;

        if (!rigidbodiesOnLadder.Contains(rb))
        {
            rigidbodiesOnLadder.Add(rb);
        }

        float vertical = Input.GetAxis("Vertical");

        rb.useGravity = false;

        //Apply climb velocity
        Vector3 climbVelocity = transform.up * vertical * climbSpeed * Time.deltaTime;
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, climbVelocity.y, rb.linearVelocity.z);
    }

    void OnCollisionExit(Collision other)
    {
        Rigidbody rb = other.collider.attachedRigidbody;
        if (rb != null)
        {
            rb.useGravity = true;
            
            rigidbodiesOnLadder.Remove(rb);
        }
    }

    public override void OnDeath()
    {
        foreach (Rigidbody rb in rigidbodiesOnLadder)
        {
            if (rb != null)
            {
                rb.useGravity = true;
            }
        }
        
        rigidbodiesOnLadder.Clear();
    }
}