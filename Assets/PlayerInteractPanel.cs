using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerInteractPanel : NetworkBehaviour
{
    public TextMeshProUGUI title;
    public ulong clientID;
    public ulong otherClientID;

}
