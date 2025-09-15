using TMPro;
using UnityEngine;


namespace Assets.RockPaperScissors
{
    public class GameState : State
    {
        RockPaperScissorsGame rpsGame;

        public TextMeshPro pickingTMP;

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
            if(IsServer)
            {
                rpsGame.SetTurns();
            }
            
        }

        public override void OnStateUpdate()
        {
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

