using UnityEngine;

public class BodyPartManager : MonoBehaviour
{
    [HideInInspector] public Transform hand;

    void Start()
    {
        hand = GetComponent<RagdollEnabler>().spine.GetChild(0).GetChild(0).GetChild(2).GetChild(0).GetChild(0).GetChild(0).GetChild(0);
    }
}
