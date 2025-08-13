using Unity.Netcode;
using UnityEngine;

public abstract class ArcadeGame : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public virtual void BeginServerRpc()
    { 

    }

}
