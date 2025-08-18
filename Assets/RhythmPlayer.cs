using Unity.Netcode;
using UnityEngine;

public class RhythmPlayer : NetworkBehaviour
{
    public override void OnGainedOwnership()
    {
        Debug.Log($"{OwnerClientId} gained ownership of this object");
    }

    public override void OnLostOwnership()
    {
        Debug.Log($"{OwnerClientId} lost ownership of this object");
    }
}
