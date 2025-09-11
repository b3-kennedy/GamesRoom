using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TMPro;

namespace Assets.Football
{
    public class MainMenu : State
    {
        FootballGame footballGame;
        public TextMeshPro connectedPlayersText;
        void Start()
        {
            if (game is FootballGame fg)
            {
                footballGame = fg;
            }
        }
        public override void OnStateEnter()
        {
            connectedPlayersText.text = "0/2";
            gameObject.SetActive(true);
        }

        public override void OnStateUpdate()
        {
            connectedPlayersText.text = $"{footballGame.connectedPlayersCount.Value}/2";
            if (footballGame.connectedPlayers.Count == 2)
            {
                footballGame.ChangeStateServerRpc(FootballGame.GameState.WAGER);
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}