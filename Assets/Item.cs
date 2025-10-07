using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [HideInInspector] public Transform cam;
    public virtual void Use() { }
    
    public virtual void AltUse() { }

    void Update()
    {
        UpdateItem();
    }
    public virtual void UpdateItem() { }

}
