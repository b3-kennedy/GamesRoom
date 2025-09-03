using TMPro;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class MainMenu : State
    {

        ArcherBattleGame archerBattleGame;
        public TextMeshPro connectedPlayersText;

        void Start()
        {
            if (game is ArcherBattleGame g)
            {
                archerBattleGame = g;
            }
        }

        public override void OnStateEnter()
        {
            gameObject.SetActive(true);
            connectedPlayersText.text = "0/2";
        }

        public override void OnStateUpdate()
        {
            connectedPlayersText.text = $"{archerBattleGame.connectedPlayersCount.Value}/2";
            if (archerBattleGame.connectedPlayers.Count == 2)
            {
                archerBattleGame.ChangeStateServerRpc(ArcherBattleGame.GameState.WAGER);
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

