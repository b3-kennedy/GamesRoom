using Unity.Netcode;
using UnityEngine;

public class ArcadeGame : NetworkBehaviour
{
    [ServerRpc(RequireOwnership = false)]
    public virtual void BeginServerRpc(ulong clientID)
    {

    }

    [ServerRpc(RequireOwnership = false)]
    public virtual void ResetServerRpc()
    {

    }

}
