using System.Runtime.CompilerServices;
using Unity.Netcode;
using UnityEngine;

public class SharedPhysicsObject : NetworkBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void ApplyPushForceServerRPC(Vector3 force, Vector3 hitPoint)
    {
        rb.AddForceAtPosition(force, hitPoint, ForceMode.Force);
    }

}
