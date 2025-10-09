using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [HideInInspector] public Transform cam;
    [HideInInspector] public GameObject player;

    public float price;

    public bool isAltUse;
    
    public virtual void Use() { }
    

    void Update()
    {
        UpdateItem();
    }
    public virtual void UpdateItem() { }

}
