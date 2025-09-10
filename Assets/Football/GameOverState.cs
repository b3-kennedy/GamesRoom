using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace Assets.Football
{
    public class GameOverState : State
    {
        FootballGame footballGame;
        public TextMeshPro winnerTMP;

        string winner;

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
            if (IsServer)
            {
                SetWinnerTextClientRpc(footballGame.gameState.winner.GetComponent<NetworkObject>().OwnerClientId);
            }
            
        }

        [ServerRpc(RequireOwnership = false)]
        public void SetWinnerServerRpc(string playerName)
        {
            winner = playerName;
            Debug.Log("winner");
        }

        [ClientRpc]
        void SetWinnerTextClientRpc(ulong objectID)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectID, out var playerObject))
            {
                string steamName = playerObject.GetComponent<SteamPlayer>().playerName;
                winnerTMP.text = $"{steamName} Wins!";
            }
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

