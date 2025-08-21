using TMPro;
using UnityEngine;

namespace Assets.Farkle
{
    public class GameOver : State
    {
        FarkleGame farkleGame;

        public TextMeshPro winnerText;

        void Start()
        {
            gameObject.SetActive(false);
            if (game is FarkleGame fg)
            {
                farkleGame = fg;
            }

        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            Debug.Log(farkleGame);
            winnerText.text = $"{farkleGame.winner.playerName} Wins!";
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

