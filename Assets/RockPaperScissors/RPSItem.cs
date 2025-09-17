
using UnityEngine;

public class RPSItem : MonoBehaviour
{

    public bool isSelected;
    public GameObject outline;

    void Start()
    {
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
