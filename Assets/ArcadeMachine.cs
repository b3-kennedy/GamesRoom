using UnityEngine;

public class ArcadeMachine : MonoBehaviour
{
    public ArcadeGame arcadeGame;

    public void StartGame()
    {
        arcadeGame.Begin();
    }
}
