
using UnityEngine;

public class RPSItem : MonoBehaviour
{

    public bool isSelected;
    GameObject outline;

    void Start()
    {
        outline = transform.GetChild(1).gameObject;
    }

    public void ChangeSelectedValue(bool value)
    {
        isSelected = value;
        if(isSelected)
        {
            outline.SetActive(true);
        }
        else
        {
            outline.SetActive(false);
        }
    }
}
