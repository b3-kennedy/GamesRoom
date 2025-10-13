using UnityEngine;

public class SoundboardItem : Item
{

    ItemManager manager;
    void Start()
    {
        manager = player.GetComponent<ItemManager>();
    }

    public override void OnEquip()
    {
        transform.SetLocalPositionAndRotation(new Vector3(0.023f, -0.00999999978f, 0.0329999998f), Quaternion.Euler(35.8539925f, 96.9758072f, 94.1845245f));
        base.OnEquip();
        Debug.Log(anim.GetLayerName(1));
        anim.SetLayerWeight(3, 1f);
    }

    public override void Use()
    {

        
    }

    public override void UpdateItem()
    {
        if (isAltUse)
        {
            manager.canSwitch = false;
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(i.ToString()))
                {
                    int index = (i == 0) ? player.GetComponent<Soundboard>().audioClips.Count - 1 : i - 1;
                    player.GetComponent<Soundboard>().PlaySoundAtIndex(index);
                }
            }
        }
        else
        {
            manager.canSwitch = true;
        }
    }

    public override void OnUnequip()
    {
        anim.SetLayerWeight(3, 0f);
    }
}
