using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Football
{
    public class GameOverState : State
    {
        FootballGame footballGame;
        public TextMeshPro winnerTMP;

        public string winner;

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

