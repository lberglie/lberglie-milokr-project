using Unity.Netcode;
using UnityEngine;

public class PlayerSharedPhysicsObject : NetworkBehaviour
{
    public float pushStrength = 10f;

    private void OnCollisionStay(Collision collision)
    {
        if (!IsOwner) return;

        SharedPhysicsObject pushableObject = collision.gameObject.GetComponent<SharedPhysicsObject>();

        if (pushableObject != null)
        {
            Vector3 pushDirection = -collision.contacts[0].normal;
            pushDirection.Normalize();
            Vector3 force = pushDirection * pushStrength;

            Vector3 appliedForce = pushDirection * pushStrength;

            // Apply the force to the object on the server
            pushableObject.ApplyPushForceServerRPC(appliedForce, collision.contacts[0].point);
        }


    }
}
