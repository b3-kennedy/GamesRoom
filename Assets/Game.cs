using Unity.Netcode;
using UnityEngine;

public class Game : NetworkBehaviour
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
