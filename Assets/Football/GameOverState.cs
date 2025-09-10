using TMPro;
using UnityEngine;

namespace Assets.Football
{
    public class GameOverState : State
    {
        FootballGame footballGame;
        public TextMeshPro winnerTMP;

        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            winnerTMP.text = $"{footballGame.gameState.winner} WINS!";
        }

        public override void OnStateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                footballGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

