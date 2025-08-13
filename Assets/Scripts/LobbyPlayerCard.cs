using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyPlayerCard : MonoBehaviour
{


    public TextMeshProUGUI nameText;
    public Image avatarImage; // Drag in a UI Image from the prefab

    ulong playerID;

    public void SetPlayerID(ulong id)
    {
        playerID = id;
    }

    public ulong GetPlayerID()
    {
        return playerID;
    }

    public void UpdateName(string playerName)
    {
        nameText.text = playerName;
    }

    public void UpdateAvatar(Texture2D texture)
    {
        if (texture != null)
        {
            avatarImage.sprite = Sprite.Create(
                FlipTextureVertically(texture),
                new Rect(0, 0, texture.height, texture.width),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.Tight,
                Vector4.zero,
                true // <-- 'false' here flips the y-axis UV
            );
        }

    }

    Texture2D FlipTextureVertically(Texture2D original)
    {
        Texture2D flipped = new Texture2D(original.width, original.height, original.format, false);

        for (int y = 0; y < original.height; y++)
        {
            flipped.SetPixels(0, y, original.width, 1, original.GetPixels(0, original.height - 1 - y, original.width, 1));
        }

        flipped.Apply();
        return flipped;
    }
}
