using UnityEngine;

public class SoundboardItem : Item
{

    ItemManager manager;
    void Start()
    {
        manager = player.GetComponent<ItemManager>();
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
}
