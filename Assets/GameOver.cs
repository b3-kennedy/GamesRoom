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
            if (IsServer)
            {
                if (farkleGame.wagerState is Wager wager)
                {
                    farkleGame.winner.GetComponent<SteamPlayer>().credits.Value += wager.wagerAmount.Value;
                }

            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                farkleGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

