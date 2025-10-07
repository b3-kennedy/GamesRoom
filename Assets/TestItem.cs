using UnityEngine;

public class TestItem : Item
{
    public override void Use()
    {
        float r = Random.Range(0f, 1f);
        float g = Random.Range(0f, 1f);
        float b = Random.Range(0f, 1f);
        float a = 1;
        GetComponent<MeshRenderer>().material.color = new Color(r,g,b,a);
    }
}
