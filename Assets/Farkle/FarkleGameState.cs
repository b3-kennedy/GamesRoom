using UnityEngine;
using Unity.Netcode;

namespace Assets.Farkle
{
    public class FarkleGameState : State
    {

        FarkleGame farkleGame;

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
            Debug.Log("gamee");
            var player1 = NetworkManager.Singleton.ConnectedClients[farkleGame.player1.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;
            var player2 = NetworkManager.Singleton.ConnectedClients[farkleGame.player2.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;

            farkleGame.player1.GetComponent<FarklePlayer>().playerScoreText.text = $"{player1.GetComponent<SteamPlayer>().playerName}: 0";
            farkleGame.player1.GetComponent<FarklePlayer>().playerScoreText.text = $"{player2.GetComponent<SteamPlayer>().playerName}: 0";
            if (IsServer)
            {
                int randomNum = Random.Range(0, 2);
                farkleGame.SetFirstTurn(randomNum);

            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

