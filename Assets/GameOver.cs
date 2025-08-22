using TMPro;
using Unity.Netcode;
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
                    var owner1ID = farkleGame.winner.GetComponent<NetworkObject>().OwnerClientId;
                    var player1 = NetworkManager.Singleton.ConnectedClients[owner1ID].PlayerObject;
                    player1.GetComponent<SteamPlayer>().credits.Value += wager.wagerAmount.Value;

                    var owner2ID = farkleGame.loser.GetComponent<NetworkObject>().OwnerClientId;
                    var player2 = NetworkManager.Singleton.ConnectedClients[owner2ID].PlayerObject;
                    player2.GetComponent<SteamPlayer>().credits.Value -= wager.wagerAmount.Value;
                }

            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

