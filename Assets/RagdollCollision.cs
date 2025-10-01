using UnityEngine;
using UnityEngine.Events;

public class RagdollCollision : MonoBehaviour
{
    [HideInInspector] public UnityEvent hitObject;
    void OnCollisionEnter(Collision other)
    {
        if(other.gameObject.layer != 9)
        {
            hitObject.Invoke();
        }
    }
}
