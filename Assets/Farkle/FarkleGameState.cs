using UnityEngine;
using Unity.Netcode;

namespace Assets.Farkle
{
    public class FarkleGameState : State
    {

        FarkleGame farkleGame;
        public int scoreToWin = 5000;

        FarklePlayer farklePlayer1;
        FarklePlayer farklePlayer2;


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
            farklePlayer1 = farkleGame.player1.GetComponent<FarklePlayer>();
            farklePlayer2 = farkleGame.player2.GetComponent<FarklePlayer>();
            farkleGame.player1.GetComponent<FarklePlayer>().playerScoreText.text = $"{player1.GetComponent<SteamPlayer>().playerName}: 0";
            farkleGame.player2.GetComponent<FarklePlayer>().playerScoreText.text = $"{player2.GetComponent<SteamPlayer>().playerName}: 0";
            farklePlayer1.playerName = player1.GetComponent<SteamPlayer>().playerName;
            farklePlayer2.playerName = player2.GetComponent<SteamPlayer>().playerName;
            if (IsServer)
            {
                int randomNum = Random.Range(0, 2);
                farkleGame.SetFirstTurn(randomNum);

            }
        }

        public override void OnStateUpdate()
        {
            if (farklePlayer1.playerScore.Value >= scoreToWin)
            {
                farkleGame.winner = farklePlayer1;
                farkleGame.ChangeStateServerRpc(FarkleGame.GameState.GAME_OVER);
            }
            else if (farklePlayer2.playerScore.Value >= scoreToWin)
            {
                farkleGame.winner = farklePlayer2;
            }
        }

        public override void OnStateExit()
        {
            gameObject.SetActive(false);
        }
    }
}

