using Unity.Netcode;
using UnityEngine;

public class Item : NetworkBehaviour
{
    [HideInInspector] public Transform cam;
    [HideInInspector] public GameObject player;
    [HideInInspector] public Animator anim;


    public float price;

    public bool isAltUse;
    
    public virtual void Use() { }
    
    public virtual void OnEquip()
    {
        if (player == null)
        {
            Debug.Log($"Player has not been assigned for item {gameObject.name}");
        }
        if (anim == null)
        {
            Debug.Log($"Animator has not been assigned for item {gameObject.name}");
        }
    }
    
    public virtual void OnUnequip()
    {
        
    }
    

    void Update()
    {
        UpdateItem();
    }
    public virtual void UpdateItem() { }

}
