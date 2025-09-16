using TMPro;
using UnityEngine;


namespace Assets.RockPaperScissors
{
    public class GameOver : State
    {
        RockPaperScissorsGame rpsGame;
        public TextMeshPro winnerTMP;

        void Start()
        {
            if (game is RockPaperScissorsGame rps)
            {
                rpsGame = rps;
            }
        }
        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                rpsGame.ResetServerRpc();
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}


