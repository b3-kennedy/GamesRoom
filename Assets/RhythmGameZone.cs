using UnityEngine;

public class RhythmGameZone : MonoBehaviour
{
    [HideInInspector] public GameObject target;

    public int zoneNumber;

    public RhythmPlayer player;

    void Start()
    {
    }

    void Update()
    {
        if (target)
        {
            SetInZone(true);
        }
        else
        {
            SetInZone(false);
        }
    }


    void SetInZone(bool value)
    {
        if (zoneNumber == 1)
        {
            player.inZone1 = value;
            player.leftTarget = target;
        }
        else if (zoneNumber == 2)
        {
            player.inZone2 = value;
            player.middleTarget = target;
        }
        else if (zoneNumber == 3)
        {
            player.inZone3 = value;
            player.rightTarget = target;
        }
    }
}
