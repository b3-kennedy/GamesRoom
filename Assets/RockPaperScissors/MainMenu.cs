using TMPro;
using UnityEngine;


namespace Assets.RockPaperScissors
{
    public class MainMenu : State
    {
        RockPaperScissorsGame rpsGame;
        public TextMeshPro connectedPlayersText;
        void Start()
        {
            if (game is RockPaperScissorsGame rps)
            {
                rpsGame = rps;
            }
        }
        public override void OnStateEnter()
        {
            connectedPlayersText.text = "0/2";
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {
            connectedPlayersText.text = $"{rpsGame.connectedPlayersCount.Value}/2";
            if (rpsGame.connectedPlayers.Count == 2)
            {
                rpsGame.ChangeStateServerRpc(RockPaperScissorsGame.GameState.WAGER);
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

