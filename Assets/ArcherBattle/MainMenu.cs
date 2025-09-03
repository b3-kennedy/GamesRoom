using TMPro;
using UnityEngine;


namespace Assets.ArcherBattle
{
    public class MainMenu : State
    {

        ArcherBattleGame archerBattleGame;
        public TextMeshPro connectedPlayersText;

        public GameObject floor;

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
            floor.gameObject.SetActive(false);
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
            archerBattleGame.AssignPlayersForWager();
            gameObject.SetActive(false);
        }
    }
}

