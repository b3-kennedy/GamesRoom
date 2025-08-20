using TMPro;
using UnityEngine;

namespace Assets.Farkle
{
    public class MainMenu : State
    {
        FarkleGame farkleGame;
        public TextMeshPro waitForPlayerText;

        void Start()
        {
            if (game is FarkleGame fg)
            {
                farkleGame = fg;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            waitForPlayerText.gameObject.SetActive(true);
            Debug.Log(farkleGame);
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }

        public override void OnStateUpdate()
        {
            waitForPlayerText.text = $"Waiting for players {farkleGame.connectedPlayersCount.Value}/2";
            if (farkleGame.connectedPlayersCount.Value == 2)
            {
                farkleGame.AssignPlayers();
                farkleGame.ChangeStateServerRpc(FarkleGame.GameState.WAGER);
            }
        }
    }
}

